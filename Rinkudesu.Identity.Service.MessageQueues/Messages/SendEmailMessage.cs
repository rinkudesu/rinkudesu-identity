using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;
using Rinkudesu.Kafka.Dotnet.Base;

namespace Rinkudesu.Identity.Service.MessageQueues.Messages;

[ExcludeFromCodeCoverage]
public class SendEmailMessage : GenericKafkaMessage
{
    /// <summary>
    /// Id of a user to send the email to. The actual email address will be loaded when sending the email.
    /// </summary>
    [JsonPropertyName("user_id")]
    public Guid UserId { get; set; }

    /// <summary>
    /// Email topic.
    /// </summary>
    [JsonPropertyName("email_topic")]
    public string EmailTopic { get; set; } = string.Empty;

    /// <summary>
    /// The actual body of an email, either in HTML or plaintext, depending on <see cref="IsHtml"/>.
    /// </summary>
    [JsonPropertyName("email_content")]
    public string EmailContent { get; set; } = string.Empty;

    /// <summary>
    /// If set to <c>true</c> the message will be treated as Html. Otherwise, it will be sent as plaintext.
    /// </summary>
    [JsonPropertyName("is_html")]
    public bool IsHtml { get; set; }

    public SendEmailMessage()
    {
    }

    public SendEmailMessage(Guid userId, string topic, string content, bool isHtml)
    {
        UserId = userId;
        EmailTopic = topic;
        EmailContent = content;
        IsHtml = isHtml;
    }

    public bool Validate()
        => !(UserId == Guid.Empty || string.IsNullOrWhiteSpace(EmailTopic) || string.IsNullOrWhiteSpace(EmailContent));
}