namespace AutoWise.CommonUtilities.Messaging.Abstractions;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(TEvent @event, CancellationToken ct = default) where TEvent : class;
}
