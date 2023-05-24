#pragma warning disable CA1852
#pragma warning disable CA1303
#pragma warning disable CA1305
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using CommandLine;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.OpenApi.Models;
using RInkudesu.Identity.Service.Common.Utilities;
using Rinkudesu.Identity.Service.Data;
using Rinkudesu.Identity.Service.Email;
using Rinkudesu.Identity.Service.Email.EmailConnector;
using Rinkudesu.Identity.Service.HostedServices;
using Rinkudesu.Identity.Service.MessageQueues.Handlers;
using Rinkudesu.Identity.Service.MessageQueues.Messages;
using Rinkudesu.Identity.Service.Middleware;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Identity.Service.Repositories;
using Rinkudesu.Identity.Service.Services;
using Rinkudesu.Identity.Service.Settings;
using Rinkudesu.Identity.Service.Utilities;
using Rinkudesu.Kafka.Dotnet;
using Rinkudesu.Kafka.Dotnet.Base;
using Rinkudesu.Services.Links.HealthChecks;
using Serilog;
using Serilog.Exceptions;
using StackExchange.Redis;
using Role = Rinkudesu.Identity.Service.Models.Role;

var result = Parser.Default.ParseArguments<InputArguments>(args)
    .WithParsed(o => {
        o.SaveAsCurrent();
    })
    .WithNotParsed(_ => {
        Console.WriteLine("Failed to parse input arguments.");
        Environment.Exit(1);
    });

if (result.Tag == ParserResultType.NotParsed)
{
    Console.WriteLine(@"Failed to parse input arguments.");
    Environment.Exit(1);
}

#if DEBUG
// this is necessary to avoid issues with environmental variables and missing connections during migration creation that most likely doesn't need them anyway
if (InputArguments.Current.NewMigrationCreation)
{
    Log.Warning("You're running the program in database migration creation mode. That means that only the database service will be initialised and the application will shut down immediately after. If that's not your intention, check your input arguments and build flags.");
    var migrationBuilder = WebApplication.CreateBuilder(args);
    migrationBuilder.Services.AddDbContext<IdentityContext>(o => o.UseNpgsql("Server=127.0.0.1;Port=5432;Database=rinku-identity;User Id=postgres;Password=postgres"));
    migrationBuilder.Build();
    return 0;
}
#endif

var logConfig = new LoggerConfiguration();
logConfig.WriteTo.Console();
Log.Logger = logConfig.CreateBootstrapLogger();

try
{
    var baseUrl = EnvironmentalVariablesReader.GetBaseUrl();

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((context, services, configuration) => {
        configuration.WriteTo.Console();
        InputArguments.Current.GetMinimumLogLevel(configuration);
        configuration.ReadFrom.Services(services);
        configuration
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", context.HostingEnvironment.ApplicationName)
            .Enrich.WithExceptionDetails();
    });

// Add services to the container.

    builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    builder.Services.AddEmailConnector();
    builder.Services.AddEmailSender();

    RedisSettings.Current = new RedisSettings();

    var redisConnectionMultiplexer = await ConnectionMultiplexer.ConnectAsync(RedisSettings.Current.Host);
    var redisConnectionProvider = new RedisConnectionProvider(redisConnectionMultiplexer);
    builder.Services.AddSingleton(redisConnectionProvider);
    builder.Services.AddDataProtection().SetApplicationName("rinkudesu").PersistKeysToStackExchangeRedis(redisConnectionMultiplexer, "DataProtection-Keys");

//register identity-related services that override identity defaults
    builder.Services.AddScoped<IPasswordHasher<User>, ArgonPasswordHasher>();

    builder.Services.AddIdentity<User, Role>().AddEntityFrameworkStores<IdentityContext>();
    builder.Services.AddScoped<IUserStore<User>, UserStore<User, Role, IdentityContext, Guid>>();
    builder.Services.AddScoped<IRoleStore<Role>, RoleStore<Role, IdentityContext, Guid>>();
    builder.Services.ConfigureApplicationCookie(o => {
        o.Cookie.Name = ".rinkudesu.session";
        o.ExpireTimeSpan = TimeSpan.FromHours(12);
        o.SessionStore = new RedisCacheTicketStore(RedisSettings.Current.GetRedisOptions());
        o.Events.OnRedirectToLogin = context => {
            context.Response.StatusCode = 401;
            return Task.CompletedTask;
        };
    });
    builder.Services.Configure<IdentityOptions>(o => {
        o.Lockout = new LockoutOptions
        {
            AllowedForNewUsers = true,
            DefaultLockoutTimeSpan = TimeSpan.FromMinutes(30),
            MaxFailedAccessAttempts = 3,
        };
        o.User = new UserOptions
        {
            RequireUniqueEmail = true,
        };
        o.SignIn = new SignInOptions
        {
            RequireConfirmedAccount = true,
            RequireConfirmedEmail = true,
        };
        o.Password = new PasswordOptions
        {
            RequireDigit = false,
            RequiredLength = 12,
            RequiredUniqueChars = 1,
            RequireLowercase = false,
            RequireUppercase = false,
            RequireNonAlphanumeric = false,
        };
    });

    builder.Services.AddDbContext<IdentityContext>(o => {
        o.UseNpgsql(EnvironmentalVariablesReader.GetRequiredVariable(EnvironmentalVariablesReader.DbContextVariableName), psql => { psql.EnableRetryOnFailure(); });
        o.ConfigureWarnings(w => w.Throw(CoreEventId.RowLimitingOperationWithoutOrderByWarning).Throw(CoreEventId.FirstWithoutOrderByAndFilterWarning).Throw(CoreEventId.DistinctAfterOrderByWithoutRowLimitingOperatorWarning));
#if DEBUG
        o.EnableSensitiveDataLogging();
#endif
    });

    RegisterOtherServices(builder);
    SetupKafka(builder);

    builder.Services.AddApiVersioning(o => {
        o.AssumeDefaultVersionWhenUnspecified = true;
        o.DefaultApiVersion = new ApiVersion(1, 0);
        o.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

    builder.Services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "User management API",
            Description = "API to manage users and identities",
        });
        var xmlName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = AppContext.BaseDirectory;
        c.IncludeXmlComments(Path.Combine(xmlPath, xmlName));
    });

    builder.Services.AddHealthChecks().AddCheck<DatabaseHealthCheck>("Database health check");

    var app = builder.Build();

// Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI(c => { c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API V1"); });
    }

    app.UseHttpsRedirection();

    app.UseSerilogRequestLogging();

    app.UseAuthorization();

    app.UseMiddleware<UserReaderMiddleware>();

    app.MapControllers();
    app.MapHealthChecks("/health");
    app.MapGet("/.well-known/openid-configuration", () => $"{{\"issuer\":\"{baseUrl}\",\"jwks_uri\":\"{baseUrl}/api/Jwt/jwks.json\"}}");

    using (var bootstrapScope = app.Services.CreateScope())
    {
        if (InputArguments.Current.ApplyMigrations)
        {
            var database = bootstrapScope.ServiceProvider.GetRequiredService<IdentityContext>();
            await database.Database.MigrateAsync();
        }
        if (EnvironmentalVariablesReader.IsDefaultUserProvided(out var initialEmail, out var initialPassword))
        {
            var userManager = bootstrapScope.ServiceProvider.GetRequiredService<UserManager<User>>();
            if (!await userManager.Users.AnyAsync())
            {
                var defaultUser = new User
                {
                    UserName = initialEmail,
                    Email = initialEmail,
                    EmailConfirmed = true,
                };
                var userCreationResult = await userManager.CreateAsync(defaultUser, initialPassword);
                if (!userCreationResult.Succeeded)
                    throw new InvalidOperationException("Failed to create default user");
            }
            else
            {
                Log.Logger.Information("Default user creation skipped as other users already exist in the database");
            }
        }
    }

    await app.RunAsync();

}
#pragma warning disable CA1031
catch (Exception e)
#pragma warning restore CA1031
{
    Log.Fatal(e, "Application failed to start");
    return 2;
}
finally
{
    await Log.CloseAndFlushAsync();
}
return 0;

void RegisterOtherServices(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton<JwtKeysRepository>();
    builder.Services.AddTransient<JwtSecurityTokenHandler>();
    builder.Services.AddScoped<SessionTicketRepository>();
}

static void SetupKafka(WebApplicationBuilder builder)
{
    var kafkaConfig = KafkaConfigurationProvider.ReadFromEnv();
    builder.Services.AddSingleton(kafkaConfig);
    builder.Services.AddSingleton<IKafkaProducer, KafkaProducer>();

    builder.Services.AddSingleton<IKafkaSubscriber<SendEmailMessage>, KafkaSubscriber<SendEmailMessage>>();
    builder.Services.AddSingleton<IKafkaSubscriberHandler<SendEmailMessage>, SendEmailMessageHandler>();
    builder.Services.AddHostedService<SendEmailHandlerService>();
}
