using System.ComponentModel.DataAnnotations;

namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Data used for user password reset using email recovery flow.
/// </summary>
public class ResetPasswordDto
{
    /// <summary>
    /// Id of the user resetting password.
    /// </summary>
    [Required]
    public Guid UserId { get; set; }
    /// <summary>
    /// Token sent to the user email.
    /// </summary>
    [Required]
    public string Token { get; set; } = string.Empty;
    /// <summary>
    /// New user password.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
    /// <summary>
    /// <see cref="Password"/> repeated.
    /// </summary>
    [Required]
    public string PasswordRepeat { get; set; } = string.Empty;

    /// <summary>
    /// Indicates that there's a mismatch between <see cref="Password"/> and <see cref="PasswordRepeat"/>.
    /// </summary>
    public bool PasswordMismatch => Password != PasswordRepeat;
}
