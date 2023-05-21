using System.Net.Mail;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Rinkudesu.Identity.Service.Email.EmailConnector;

namespace Rinkudesu.Identity.Service.Email.Tests;

public class EmailSenderTests
{
    private readonly EmailSender _emailSender;
    private readonly Mock<IEmailConnector> _mockConnector;

    public EmailSenderTests()
    {
        _mockConnector = new Mock<IEmailConnector>(MockBehavior.Strict);
        _mockConnector.SetupGet(c => c.From).Returns(new MailAddress("test@localhost.localdomain"));
        _emailSender = new EmailSender(NullLogger<EmailSender>.Instance, _mockConnector.Object);
    }

    [Theory]
    [InlineData("recipient@localhost.localdomain", "test content", "hello")]
    public async Task SendMessage_NoCcBcc_MessageSent(string to, string content, string subject)
    {
        var options = new EmailOptions(to, subject, content, false);
        _mockConnector.Setup(c => c.SendMailAsync
        (
            It.Is<MailMessage>(m => m.To.Single().Address == to && m.Body == content && m.Subject == subject && !m.CC.Any() && !m.Bcc.Any()),
            It.IsAny<CancellationToken>()
        )).Returns(Task.CompletedTask).Verifiable();

        await _emailSender.SendMessage(options);

        _mockConnector.VerifyAll();
    }

    [Theory]
    [InlineData("recipient@localhost.localdomain", "test content", "hello", new [] {"cc1@localhost.localdomain"}, new [] {"bcc1@localhost.localdomain"})]
    [InlineData("recipient@localhost.localdomain", "test content", "hello", new [] {"cc1@localhost.localdomain", "cc2@localhost.localdomain"}, new [] {"bcc1@localhost.localdomain"})]
    [InlineData("recipient@localhost.localdomain", "test content", "hello", new [] {"cc1@localhost.localdomain"}, new [] {"bcc1@localhost.localdomain", "bcc2@localhost.localdomain"})]
    public async Task SendMessage_CcBccBccSet_MessageSent(string to, string content, string subject, string[] cc, string[] bcc)
    {
        var options = new EmailOptions(to, subject, content, false, cc: string.Join(',', cc), bcc: string.Join(',', bcc));
        _mockConnector.Setup(c => c.SendMailAsync
        (
            It.Is<MailMessage>(m => m.To.Single().Address == to && m.Body == content && m.Subject == subject
                                    && m.CC.All(c => cc.Contains(c.Address)) && cc.All(c => m.CC.Select(ccc => ccc.Address).Contains(c))
                                    && m.Bcc.All(c => bcc.Contains(c.Address)) && bcc.All(c => m.Bcc.Select(ccc => ccc.Address).Contains(c))
                                     ),
            It.IsAny<CancellationToken>()
        )).Returns(Task.CompletedTask).Verifiable();

        await _emailSender.SendMessage(options);

        _mockConnector.VerifyAll();
    }
}
