using WindowShopper.Orders.Commands.Features.Events;

namespace WindowShopper.Orders.Commands.Features.CreateOrder.CreateOrderSaga
{
    public interface ICreateOrderSaga
    {
        Task StartSaga(Guid OrderId);
        Task HandleOrderCreatedEvent(OrderCreatedEvent orderCreatedEvent, CancellationToken cancellationToken);
        Task HandleOrderConfirmedEvent(OrderConfirmedEvent orderConfirmedEvent, CancellationToken cancellationToken);
        Task HandleOrderCancelledEvent(OrderCancelledEvent orderCancelledEvent, CancellationToken cancellationToken);
    }
}
