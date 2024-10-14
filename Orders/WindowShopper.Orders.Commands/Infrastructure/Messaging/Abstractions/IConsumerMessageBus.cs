namespace WindowShopper.Orders.Commands.Infrastructure.Messaging.Abstractions;

public interface IConsumerMessageBus<T>
{
    T? Consume(CancellationToken cancellationToken);
}