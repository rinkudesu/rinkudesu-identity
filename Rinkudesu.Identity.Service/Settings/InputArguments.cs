using System.Diagnostics.CodeAnalysis;
using CommandLine;
using Serilog;
// ReSharper disable ClassNeverInstantiated.Global
#pragma warning disable CS1591

namespace Rinkudesu.Identity.Service.Settings;

[ExcludeFromCodeCoverage]
public class InputArguments
{
    public static InputArguments Current { get; private set; } = null!; // the value is always set as the first operation of the program

    [Option(longName: "applyMigrations", Required = false, HelpText = "Automatically creates the database and applies any missing migrations on startup")]
    public bool ApplyMigrations { get; set; }
    [Option(shortName: 'l', longName: "logLevel", Required = false, Default = 2, HelpText = "Defines log level: 0 - log everything, 5 - log errors only")]
    public int LogLevel { get; set; }

#if DEBUG
    // not sure what that is but is apparently set during migration creation, so to avoid issues let's just set it here
    [Option(longName: "applicationName", Required = false, HelpText = "DON'T USE: this is an argument that's inexplicably passed by ef when creating a new migration. When set, the application will treat it as a sign that a new migration is being created and will not start.")]
    public string ApplicationName { get; set; } = string.Empty;
    public bool NewMigrationCreation => !string.IsNullOrWhiteSpace(ApplicationName);
#endif

    public void SaveAsCurrent()
    {
        Current = this;
    }

    public LoggerConfiguration GetMinimumLogLevel(LoggerConfiguration configuration)
    {
        return LogLevel switch
        {
            0 => configuration.MinimumLevel.Verbose(),
            1 => configuration.MinimumLevel.Debug(),
            2 => configuration.MinimumLevel.Information(),
            3 => configuration.MinimumLevel.Warning(),
            4 => configuration.MinimumLevel.Error(),
            _ => configuration.MinimumLevel.Information(),
        };
    }
}
