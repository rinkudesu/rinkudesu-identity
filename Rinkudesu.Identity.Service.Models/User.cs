using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace Rinkudesu.Identity.Service.Models;

/// <inheritdoc/>
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
}
