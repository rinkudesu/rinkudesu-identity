namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Data indicating that the user account was successfully created.
/// </summary>
public class AccountCreatedDto
{
    /// <summary>
    /// Id of the newly created account.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// Confirmation token for the user email.
    /// </summary>
    /// <remarks>
    /// Note that if account confirmation is required, the user will not be able to log in without confirming the email using this token first.
    /// </remarks>
    public string EmailConfirmationToken { get; set; } = string.Empty;
}
