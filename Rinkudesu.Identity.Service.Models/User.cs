using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;

namespace Rinkudesu.Identity.Service.Models;

/// <inheritdoc/>
[ExcludeFromCodeCoverage]
public class User : IdentityUser<Guid>
{
    #region database type changes from text to varchar

    [MaxLength(2048), DataType(DataType.Password)]
    public override string? PasswordHash { get; set; }
    [MaxLength(500)]
    public override string? SecurityStamp { get; set; }
    [MaxLength(256)]
    public override string? ConcurrencyStamp { get; set; }
    [MaxLength(100), DataType(DataType.PhoneNumber)]
    public override string? PhoneNumber { get; set; }

    #endregion

    public User()
    {
    }

    public static User CreateWithEmail(string email)
        => new User
        {
            Email = email,
            UserName = email,
        };
}
