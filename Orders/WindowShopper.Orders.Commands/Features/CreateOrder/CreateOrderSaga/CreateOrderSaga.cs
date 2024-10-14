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
    private readonly ILogger<CreateOrderSaga> _logger;

    public CreateOrderSaga(IProducerMessageBus producerMessageBus, OrdersDbContext ordersDbContext, ILogger<CreateOrderSaga> logger)
    {
        _producerMessageBus = producerMessageBus;
        _ordersDbContext = ordersDbContext;
        _logger = logger;
    }

    public async Task StartSaga(Guid OrderId, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Starting Saga for OrderId: {OrderId}", OrderId);
        _ordersDbContext.Sagas.Add(new SagaDataModel
        {
            CorrelationId = Guid.NewGuid(),
            SagaCurrentStep = SagaCurrentStep.SAGA_STARTED.ToString(),
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

        _logger.LogWarning("Sending {messageStr} to {topic}", messageStr, topic);
        await _producerMessageBus.ProduceAsync(topic, messageStr, cancellationToken);
    }

    public async Task HandleOrderCancelledEvent(OrderCancelledEvent orderCancelledEvent, CancellationToken cancellationToken)
    {
        // save the new state of the order to the db and also the saga state. This should be a bulk update for best performance
        _logger.LogWarning("Setting Order {OrderId} as Cancelled in the Orders Table", orderCancelledEvent.orderId);
        await _ordersDbContext.Orders
            .Where(order => order.OrderId == orderCancelledEvent.orderId)
            .ExecuteUpdateAsync(order => 
                order.SetProperty(o => o.OrderStatus, OrderStatus.CANCELLED), cancellationToken);
        
        _logger.LogWarning("Setting saga CurrentStep as {SagaCurrentStep} in the Sagas Table", SagaCurrentStep.SAGA_COMPLETE.ToString());
        await _ordersDbContext.Sagas
            .Where(saga => saga.OrderId == orderCancelledEvent.orderId)
            .ExecuteUpdateAsync(order => 
                order.SetProperty(o => o.SagaCurrentStep, SagaCurrentStep.SAGA_COMPLETE.ToString()), 
                cancellationToken: cancellationToken);

        // for visualization purposes
        _logger.LogDebug("Storing Order Event State Change");
        _ordersDbContext.OrderEvents.Add(new OrderEventsDataModel
        {
            OrderId = orderCancelledEvent.orderId,
            EventName = nameof(OrderCancelledEvent),
            OrderStatus = OrderStatus.CANCELLED,
            CreatedAt = DateTime.UtcNow
        });

        await _ordersDbContext.SaveChangesAsync(cancellationToken);
        
        // end of saga (we could then publish to a send order confirmed email for example)
        _logger.LogWarning("Saga finished for {OrderId} with status: CANCELLED!", orderCancelledEvent.orderId);

        _logger.LogWarning("Notifying the Customer of the result");
        await _producerMessageBus.ProduceAsync("order-notifications-topic", 
            $"OrderCancelled:{orderCancelledEvent.orderId}", cancellationToken);
    }

    public async Task HandleOrderConfirmedEvent(OrderConfirmedEvent orderConfirmedEvent, CancellationToken cancellationToken)
    {
        // save the new state of the order to the db
        _logger.LogWarning("Setting Order {OrderId} as Cancelled in the Orders Table", orderConfirmedEvent.orderId);
        await _ordersDbContext.Orders
            .Where(order => order.OrderId == orderConfirmedEvent.orderId)
            .ExecuteUpdateAsync(order => 
                order.SetProperty(o => o.OrderStatus, OrderStatus.CONFIRMED), cancellationToken);

        _logger.LogWarning("Setting saga CurrentStep as {SagaCurrentStep} in the Sagas Table", SagaCurrentStep.SAGA_COMPLETE.ToString());
        await _ordersDbContext.Sagas
            .Where(saga => saga.OrderId == orderConfirmedEvent.orderId)
            .ExecuteUpdateAsync(order => 
                order.SetProperty(o => o.SagaCurrentStep, 
                    SagaCurrentStep.SAGA_COMPLETE.ToString()), 
                cancellationToken);
        
        // for visualization purposes
        _logger.LogDebug("Storing Order Event State Change");
        _ordersDbContext.OrderEvents.Add(new OrderEventsDataModel
        {
            OrderId = orderConfirmedEvent.orderId,
            EventName = nameof(OrderConfirmedEvent),
            OrderStatus = OrderStatus.CONFIRMED,
            CreatedAt = DateTime.UtcNow
        });
        
        _ordersDbContext.SaveChangesAsync(cancellationToken);

        // end of saga (we could then publish to a send order confirmed email for example)
        _logger.LogWarning("Saga finished for {OrderID} with status: CONFIRMED!", orderConfirmedEvent.orderId);
        
        _logger.LogWarning("Notifying the Customer of the result");
        await _producerMessageBus.ProduceAsync("order-notifications-topic", 
            $"OrderConfirmed:{orderConfirmedEvent.orderId}", 
            cancellationToken);
    }
}
