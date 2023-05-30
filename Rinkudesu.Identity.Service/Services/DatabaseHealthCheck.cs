using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Rinkudesu.Identity.Service.Data;
#pragma warning disable CS1591

namespace Rinkudesu.Services.Links.HealthChecks
{
    [ExcludeFromCodeCoverage]
    public class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IdentityContext _context;

        public DatabaseHealthCheck(IdentityContext context)
        {
            _context = context;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
        {
            if (!await _context.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false))
            {
                return HealthCheckResult.Unhealthy("Unable to connect to the database");
            }
            var migrations = await _context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false);
            if (migrations.Any())
            {
                return HealthCheckResult.Degraded("Database migrations are pending, functionality may be limited");
            }
            return HealthCheckResult.Healthy();
        }
    }
}
