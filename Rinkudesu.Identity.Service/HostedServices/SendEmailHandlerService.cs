using System.Diagnostics.CodeAnalysis;
using Rinkudesu.Identity.Service.MessageQueues.Messages;
using Rinkudesu.Kafka.Dotnet.Base;
#pragma warning disable CS1591

namespace Rinkudesu.Identity.Service.HostedServices;

/// <summary>
/// Kafka message handler for sending emails.
/// </summary>
[ExcludeFromCodeCoverage]
public sealed class SendEmailHandlerService : IHostedService, IAsyncDisposable
{
    private readonly IKafkaSubscriber<SendEmailMessage> _subscriber;
    private readonly IKafkaSubscriberHandler<SendEmailMessage> _handler;

    private readonly CancellationTokenSource _cancellationToken;

    public SendEmailHandlerService(IKafkaSubscriber<SendEmailMessage> subscriber, IKafkaSubscriberHandler<SendEmailMessage> handler)
    {
        _subscriber = subscriber;
        _handler = handler;
        _cancellationToken = new CancellationTokenSource();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _subscriber.Subscribe(_handler);
        _subscriber.BeginHandle(_cancellationToken.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _cancellationToken.Cancel();
        await _subscriber.StopHandle();
        await _subscriber.Unsubscribe();
    }

    public async ValueTask DisposeAsync()
    {
        _cancellationToken.Cancel();
        await _subscriber.DisposeAsync();
        _cancellationToken.Dispose();
    }
}
