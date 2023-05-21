using System.Net.Mail;

namespace Rinkudesu.Identity.Service.Email.EmailConnector;

public interface IEmailConnector : IDisposable
{
    SmtpClient Client { get; }
}
