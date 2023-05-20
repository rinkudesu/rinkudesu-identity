namespace Rinkudesu.Identity.Service.Email;

public interface IEmailSender
{
    Task SendMessage(EmailOptions options);
    // this could have default implementation right here, but that would make logging the exceptions impossible, so will leave that for the actual implementation
    Task<bool> TrySendMessage(EmailOptions options);
}
