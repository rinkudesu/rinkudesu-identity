using System.Net.Mail;

namespace Rinkudesu.Identity.Service.Email.EmailConnector;

public interface IEmailConnector : IDisposable
{
    MailAddress From { get; }
    Task SendMailAsync(MailMessage message, CancellationToken cancellationToken = default);
}
