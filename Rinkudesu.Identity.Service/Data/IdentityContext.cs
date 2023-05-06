#pragma warning disable CS1591
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Rinkudesu.Identity.Service.Models;

namespace Rinkudesu.Identity.Service.Data;

public class IdentityContext : IdentityDbContext<User, Role, Guid>
{
    public IdentityContext(DbContextOptions<IdentityContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        #region database type changes for tables without explicit classes
        // In the perfect world all of those classes should be inherited and have those limits specified explicitly,
        // but that's too much work for only applying database types, so I'll do it this way.

        builder.Entity<IdentityRoleClaim<Guid>>().Property(t => t.ClaimType).HasMaxLength(500);
        builder.Entity<IdentityRoleClaim<Guid>>().Property(t => t.ClaimValue).HasMaxLength(500);

        builder.Entity<IdentityUserClaim<Guid>>().Property(t => t.ClaimType).HasMaxLength(500);
        builder.Entity<IdentityUserClaim<Guid>>().Property(t => t.ClaimValue).HasMaxLength(500);

        builder.Entity<IdentityUserLogin<Guid>>().Property(t => t.LoginProvider).HasMaxLength(500);
        builder.Entity<IdentityUserLogin<Guid>>().Property(t => t.ProviderKey).HasMaxLength(500);
        builder.Entity<IdentityUserLogin<Guid>>().Property(t => t.ProviderDisplayName).HasMaxLength(500);

        builder.Entity<IdentityUserToken<Guid>>().Property(t => t.LoginProvider).HasMaxLength(500);
        builder.Entity<IdentityUserToken<Guid>>().Property(t => t.Name).HasMaxLength(500);
        builder.Entity<IdentityUserToken<Guid>>().Property(t => t.Value).HasMaxLength(1000);

        #endregion
    }
}
