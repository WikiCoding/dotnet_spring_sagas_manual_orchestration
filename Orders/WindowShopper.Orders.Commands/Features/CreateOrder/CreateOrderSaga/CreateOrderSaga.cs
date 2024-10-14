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
    private readonly OrderCompletedConsumer _invetoryDeductedConsumer;

    public CreateOrderSaga(IProducerMessageBus producerMessageBus, OrderCompletedConsumer invetoryDeductedConsumer, OrdersDbContext ordersDbContext)
    {
        _producerMessageBus = producerMessageBus;
        _invetoryDeductedConsumer = invetoryDeductedConsumer;
        _ordersDbContext = ordersDbContext;
    }

    public async Task StartSaga(Guid OrderId)
    {
        _ordersDbContext.Sagas.Add(new SagaDataModel
        {
            SagaCurrentStep = "OrderCreated",
            OrderId = OrderId
        });

        await _ordersDbContext.SaveChangesAsync();
    }

    public async Task HandleOrderCreatedEvent(OrderCreatedEvent notification, CancellationToken cancellationToken)
    {
        const string topic = "order-created-topic";
        var messageStr = JsonSerializer.Serialize(notification);

        await StartSaga(notification.orderId);

        await _producerMessageBus.ProduceAsync(topic, messageStr, cancellationToken);
    }

    public async Task HandleOrderCancelledEvent(OrderCancelledEvent orderCancelledEvent, CancellationToken cancellationToken)
    {
        // save the new state of the order to the db
        int rowsAffected = await _ordersDbContext.Orders
            .Where(order => order.OrderId == orderCancelledEvent.orderId)
            .ExecuteUpdateAsync(order => order.SetProperty(o => o.OrderStatus, OrderStatus.CANCELLED));

        int rowsAffected1 = await _ordersDbContext.Sagas
            .Where(saga => saga.OrderId == orderCancelledEvent.orderId)
            .ExecuteUpdateAsync(order => order.SetProperty(o => o.SagaCurrentStep, "OrderCancelled"));

        Console.WriteLine($"Updated Order status result {rowsAffected}");
        Console.WriteLine($"Updated Saga status result {rowsAffected1}");
        
        // end of saga (we could then publish to a send order confirmed email for example)
        Console.WriteLine($"Saga finished for {orderCancelledEvent.orderId} with status: CANCELLED!");
    }

    public async Task HandleOrderConfirmedEvent(OrderConfirmedEvent orderConfirmedEvent, CancellationToken cancellationToken)
    {
        // save the new state of the order to the db
        await _ordersDbContext.Orders
            .Where(order => order.OrderId == orderConfirmedEvent.orderId)
            .ExecuteUpdateAsync(order => order.SetProperty(o => o.OrderStatus, OrderStatus.CONFIRMED));

        await _ordersDbContext.Sagas
            .Where(saga => saga.OrderId == orderConfirmedEvent.orderId)
            .ExecuteUpdateAsync(order => order.SetProperty(o => o.SagaCurrentStep, "OrderCompleted"));

        // end of saga (we could then publish to a send order confirmed email for example)
        Console.WriteLine($"Saga finished for {orderConfirmedEvent.orderId} with status: CONFIRMED!");
    }
}
