using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Rinkudesu.Identity.Service.Models;

/// <inheritdoc/>
[ExcludeFromCodeCoverage]
public class Role : IdentityRole<Guid>
{
    [MaxLength(256)]
    public override string? ConcurrencyStamp { get; set; }

    public enum Roles
    {
        Admin,
    }

    [SuppressMessage("Design", "CA1034:Nested types should not be visible", Justification = "This is essentially a glorified enum needed for controllers 'Authorise' attribute")]
    public static class RoleNames
    {
        public const string Admin = "Admin";
    }

    internal static class RoleIds
    {
        public static readonly Guid Admin = new Guid("f513acca-f3be-4461-9d5e-41c9f8348c16");
    }
}

[ExcludeFromCodeCoverage]
public static class RolesExtensions
{
    public static string GetRoleName(this Role.Roles role)
        => role switch
        {
            Role.Roles.Admin => Role.RoleNames.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(role), "Unknown role"),
        };

    public static Guid GetRoleId(this Role.Roles role)
        => role switch
        {
            Role.Roles.Admin => Role.RoleIds.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(role), "Unknown role"),
        };
}
