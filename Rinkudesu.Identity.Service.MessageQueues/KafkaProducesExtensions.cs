using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Identity.Service.MessageQueues.Messages;
using Rinkudesu.Kafka.Dotnet.Base;

namespace Rinkudesu.Identity.Service.MessageQueues;

[ExcludeFromCodeCoverage]
public static class KafkaProducesExtensions
{
    public static async Task ProduceUserDeleted(this IKafkaProducer producer, Guid userId, CancellationToken cancellationToken = default)
        => await producer.Produce(Constants.TOPIC_USER_DELETED, new UserDeletedMessage(userId), cancellationToken).ConfigureAwait(false);
}
