using AutoWise.CommonUtilities.Messaging.Abstractions;
using MassTransit;

namespace AutoWise.CommonUtilities.Messaging.MassTransit;

public class MassTransitEventPublisher(IPublishEndpoint publishEndpoint) : IEventPublisher
{
    public Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class
    {
        return publishEndpoint.Publish(@event, ct);
    }
}
