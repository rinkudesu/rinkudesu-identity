using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Rinkudesu.Identity.Service.Email.EmailConnector;

namespace Rinkudesu.Identity.Service.Email;

internal class EmailSender : IEmailSender
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IEmailConnector _emailConnector;

    public EmailSender(ILogger<EmailSender> logger, IEmailConnector emailConnector)
    {
        _logger = logger;
        _emailConnector = emailConnector;
    }

    public async Task SendMessage(EmailOptions options, CancellationToken cancellationToken = default)
    {
        using var smtpMessage = new MailMessage("test@localhost.localdomain", options.To)
        {
            Body = options.Content,
            IsBodyHtml = options.IsContentHtml,
            Subject = options.Subject,
        };
        if (!string.IsNullOrWhiteSpace(options.Cc))
            smtpMessage.CC.Add(options.Cc);
        if (!string.IsNullOrWhiteSpace(options.Bcc))
            smtpMessage.Bcc.Add(options.Bcc);

        _logger.LogInformation("Attempting to send email");
        await _emailConnector.Client.SendMailAsync(smtpMessage, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public async Task<bool> TrySendMessage(EmailOptions options, CancellationToken cancellationToken = default)
    {
        try
        {
            await SendMessage(options, cancellationToken: cancellationToken).ConfigureAwait(false);
            return true;
        }
#pragma warning disable CA1031 //purposefully avoiding throwing anything
        catch (Exception e)
#pragma warning restore CA1031
        {
            _logger.LogWarning(e, "Failed to send email message to email {Email}", options.To);
            return false;
        }
    }
}
