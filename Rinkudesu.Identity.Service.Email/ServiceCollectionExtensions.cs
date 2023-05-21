using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Rinkudesu.Identity.Service.Email.EmailConnector;

namespace Rinkudesu.Identity.Service.Email;

[ExcludeFromCodeCoverage]
public static class ServiceCollectionExtensions
{
    public static void AddEmailConnector(this IServiceCollection serviceCollection)
        => serviceCollection.AddTransient<IEmailConnector, EmailConnector.EmailConnector>();

    public static void AddEmailSender(this IServiceCollection serviceCollection)
        => serviceCollection.AddTransient<IEmailSender, EmailSender>();
}
