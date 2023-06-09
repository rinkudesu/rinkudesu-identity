using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Rinkudesu.Identity.Service.Email;
using Rinkudesu.Identity.Service.MessageQueues.Messages;
using Rinkudesu.Identity.Service.Models;
using Rinkudesu.Kafka.Dotnet.Base;

namespace Rinkudesu.Identity.Service.MessageQueues.Handlers;

[ExcludeFromCodeCoverage]
public class SendEmailMessageHandler : IKafkaSubscriberHandler<SendEmailMessage>
{
    private IEmailSender? _emailSender;
    private UserManager<User>? _userManager;

    public string Topic => Constants.TOPIC_SEND_EMAIL;

    public async Task<bool> Handle(SendEmailMessage rawMessage, CancellationToken cancellationToken = default)
    {
        if (rawMessage.UserId == Guid.Empty && string.IsNullOrWhiteSpace(rawMessage.ForceAnotherEmail))
            return true; //malformed message

        var email = rawMessage.ForceAnotherEmail ?? string.Empty;

        if (rawMessage.UserId != Guid.Empty)
        {
            var user = await _userManager!.FindByIdAsync(rawMessage.UserId.ToString()).ConfigureAwait(false);

            //consider this as if the message has been sent properly, as this probably means the user no longer exists
            //or something along those lines, which would mean that this will never send, blocking the queue
            if (user is null)
                return true;
            email = user.Email!;
        }

        var emailOptions = new EmailOptions(email, rawMessage.EmailSubject, rawMessage.EmailContent, rawMessage.IsHtml);
        return await _emailSender!.TrySendMessage(emailOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public SendEmailMessage Parse(string rawMessage)
        => System.Text.Json.JsonSerializer.Deserialize<SendEmailMessage>(rawMessage)
           ?? throw new FormatException("Unable to parse send email message");
    public IKafkaSubscriberHandler<SendEmailMessage> SetScope(IServiceScope serviceScope)
    {
        _emailSender = serviceScope.ServiceProvider.GetRequiredService<IEmailSender>();
        _userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<User>>();
        return this;
    }
}
