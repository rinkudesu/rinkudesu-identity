namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Data required to set a new password using email recovery.
/// </summary>
public class PasswordRecoveryDto
{
    /// <summary>
    /// Id of the user resetting the password.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Token that will be used for password reset.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
