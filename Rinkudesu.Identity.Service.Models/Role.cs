﻿using System.ComponentModel.DataAnnotations;
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

    internal static class RoleNames
    {
        public const string Admin = "Admin";
    }
}

public static class RolesExtensions
{
    public static string GetRoleName(this Role.Roles role)
        => role switch
        {
            Role.Roles.Admin => Role.RoleNames.Admin,
            _ => throw new ArgumentOutOfRangeException(nameof(role), "Unknown role"),
        };
}
