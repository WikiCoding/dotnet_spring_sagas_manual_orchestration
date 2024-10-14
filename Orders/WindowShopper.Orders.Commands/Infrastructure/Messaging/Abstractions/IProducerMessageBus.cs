namespace WindowShopper.Orders.Commands.Infrastructure.Messaging.Abstractions;

public interface IProducerMessageBus
{
    Task ProduceAsync(string topic, string message, CancellationToken cancellationToken);
}