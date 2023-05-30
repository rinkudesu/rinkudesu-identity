namespace Rinkudesu.Identity.Service.DataTransferObjects;

/// <summary>
/// Data send with details regarding email change.
/// </summary>
public class EmailChangeConfirmationDto
{
    /// <summary>
    /// Id of the user trying to change email.
    /// </summary>
    public Guid UserId { get; set; }
    /// <summary>
    /// New email to send the confirmation to.
    /// </summary>
    public string NewEmail { get; set; }
    /// <summary>
    /// Token to send to <see cref="NewEmail"/> for validation.
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// Initialises the confirmation data.
    /// </summary>
    public EmailChangeConfirmationDto(Guid userId, string newEmail, string token)
    {
        UserId = userId;
        NewEmail = newEmail;
        Token = token;
    }
}
