using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Identity.Service.MessageQueues.Messages;
using Rinkudesu.Kafka.Dotnet.Base;

namespace Rinkudesu.Identity.Service.MessageQueues;

[ExcludeFromCodeCoverage]
public static class KafkaProducesExtensions
{
    public static async Task ProduceUserDeleted(this IKafkaProducer producer, Guid userId, CancellationToken cancellationToken = default)
        => await producer.Produce(Constants.TOPIC_USER_DELETED, new UserDeletedMessage(userId), cancellationToken).ConfigureAwait(false);

    public static async Task ProduceSendEmail(this IKafkaProducer producer, Guid toUser, string subject, string content, bool isHtml)
        => await producer.Produce(Constants.TOPIC_SEND_EMAIL, new SendEmailMessage(toUser, subject, content, isHtml)).ConfigureAwait(false);
}
