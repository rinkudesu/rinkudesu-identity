namespace Rinkudesu.Identity.Service.DataTransferObjects;

public class PasswordRecoveryDto
{
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
}
