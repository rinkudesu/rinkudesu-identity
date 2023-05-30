using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Password change data.
/// </summary>
[ExcludeFromCodeCoverage]
public class PasswordChangeDto
{
    /// <summary>
    /// Current password used by user.
    /// </summary>
    [Required]
    public string OldPassword { get; set; } = string.Empty;
    /// <summary>
    /// New password the user wants to set.
    /// </summary>
    [Required]
    public string NewPassword { get; set; } = string.Empty;
    /// <summary>
    /// <see cref="NewPassword"/> repeated.
    /// </summary>
    [Required]
    public string NewPasswordRepeat { get; set; } = string.Empty;

    /// <summary>
    /// Returns <c>true</c> if <see cref="NewPassword"/> and <see cref="NewPasswordRepeat"/> are equal.
    /// </summary>
    public bool NewPasswordsMatch => NewPassword == NewPasswordRepeat;
}
