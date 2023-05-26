namespace Rinkudesu.Identity.Service.DataTransferObjects;

public class AccountCreatedDto
{
    public Guid UserId { get; set; }
    public string EmailConfirmationToken { get; set; }
}
