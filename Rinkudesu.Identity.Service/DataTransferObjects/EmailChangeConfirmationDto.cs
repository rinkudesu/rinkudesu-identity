namespace Rinkudesu.Identity.Service.DataTransferObjects;

public class EmailChangeConfirmationDto
{
    public Guid UserId { get; set; }
    public string NewEmail { get; set; }
    public string Token { get; set; }

    public EmailChangeConfirmationDto(Guid userId, string newEmail, string token)
    {
        UserId = userId;
        NewEmail = newEmail;
        Token = token;
    }
}
