using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WindowShopper.Orders.Commands.Features.Events;
using WindowShopper.Orders.Commands.Infrastructure.Messaging.Abstractions;
using WindowShopper.Orders.Commands.Infrastructure.Messaging.Consumers;
using WindowShopper.Orders.Commands.Repository;

namespace WindowShopper.Orders.Commands.Features.CreateOrder.CreateOrderSaga;

public class CreateOrderSaga : ICreateOrderSaga
{
    private readonly OrdersDbContext _ordersDbContext;
    private readonly IProducerMessageBus _producerMessageBus;

    public CreateOrderSaga(IProducerMessageBus producerMessageBus, OrdersDbContext ordersDbContext)
    {
        _producerMessageBus = producerMessageBus;
        _ordersDbContext = ordersDbContext;
    }

    public async Task StartSaga(Guid OrderId, CancellationToken cancellationToken)
    {
        _ordersDbContext.Sagas.Add(new SagaDataModel
        {
            SagaCurrentStep = "OrderCreated",
            OrderId = OrderId
        });

        _ordersDbContext.OrderEvents.Add(new OrderEventsDataModel
        {
            OrderId = OrderId,
            EventName = nameof(OrderCreatedEvent),
            OrderStatus = OrderStatus.PENDING_CONFIRMATION,
            CreatedAt = DateTime.UtcNow
        });

        await _ordersDbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task HandleOrderCreatedEvent(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        const string topic = "order-created-topic";
        var messageStr = JsonSerializer.Serialize(notification);

        await StartSaga(notification.orderId, cancellationToken);

        await _producerMessageBus.ProduceAsync(topic, messageStr, cancellationToken);
    }

    public async Task HandleOrderCancelledEvent(OrderCancelledEvent orderCancelledEvent, CancellationToken cancellationToken)
    {
        // save the new state of the order to the db and also the saga state. This should be a bulk update for best perfomance
        await _ordersDbContext.Orders
            .Where(order => order.OrderId == orderCancelledEvent.orderId)
            .ExecuteUpdateAsync(order => 
                order.SetProperty(o => o.OrderStatus, OrderStatus.CANCELLED), cancellationToken);

        await _ordersDbContext.Sagas
            .Where(saga => saga.OrderId == orderCancelledEvent.orderId)
            .ExecuteUpdateAsync(order => 
                order.SetProperty(o => o.SagaCurrentStep, "OrderCancelled"), cancellationToken: cancellationToken);

        // for visualization purposes
        _ordersDbContext.OrderEvents.Add(new OrderEventsDataModel
        {
            OrderId = orderCancelledEvent.orderId,
            EventName = nameof(OrderCancelledEvent),
            OrderStatus = OrderStatus.CANCELLED,
            CreatedAt = DateTime.UtcNow
        });

        await _ordersDbContext.SaveChangesAsync(cancellationToken);
        
        // end of saga (we could then publish to a send order confirmed email for example)
        Console.WriteLine($"Saga finished for {orderCancelledEvent.orderId} with status: CANCELLED!");
    }

    public async Task HandleOrderConfirmedEvent(OrderConfirmedEvent orderConfirmedEvent, CancellationToken cancellationToken)
    {
        // save the new state of the order to the db
        await _ordersDbContext.Orders
            .Where(order => order.OrderId == orderConfirmedEvent.orderId)
            .ExecuteUpdateAsync(order => 
                order.SetProperty(o => o.OrderStatus, OrderStatus.CONFIRMED), cancellationToken);

        await _ordersDbContext.Sagas
            .Where(saga => saga.OrderId == orderConfirmedEvent.orderId)
            .ExecuteUpdateAsync(order => 
                order.SetProperty(o => o.SagaCurrentStep, "OrderCompleted"), cancellationToken);
        
        // for visualization purposes
        _ordersDbContext.OrderEvents.Add(new OrderEventsDataModel
        {
            OrderId = orderConfirmedEvent.orderId,
            EventName = nameof(OrderConfirmedEvent),
            OrderStatus = OrderStatus.CONFIRMED,
            CreatedAt = DateTime.UtcNow
        });


        _ordersDbContext.SaveChangesAsync(cancellationToken);

        // end of saga (we could then publish to a send order confirmed email for example)
        Console.WriteLine($"Saga finished for {orderConfirmedEvent.orderId} with status: CONFIRMED!");
    }
}
