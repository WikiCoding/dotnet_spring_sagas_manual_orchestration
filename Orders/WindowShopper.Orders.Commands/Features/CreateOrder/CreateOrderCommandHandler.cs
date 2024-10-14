using MediatR;
using WindowShopper.Orders.Commands.Features.Commands;
using WindowShopper.Orders.Commands.Features.CreateOrder.CreateOrderSaga;
using WindowShopper.Orders.Commands.Features.Events;
using WindowShopper.Orders.Commands.Repository;

namespace WindowShopper.Orders.Commands.Features.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderCreatedEvent>
{
    private readonly OrdersDbContext _orderRepository;
    private readonly ICreateOrderSaga _createOrderSaga;
    private readonly ILogger<CreateOrderCommandHandler> _logger;

    public CreateOrderCommandHandler(OrdersDbContext orderRepository, ICreateOrderSaga createOrderSaga, ILogger<CreateOrderCommandHandler> logger)
    {
        _orderRepository = orderRepository;
        _createOrderSaga = createOrderSaga;
        _logger = logger;
    }

    public async Task<OrderCreatedEvent> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Saving order {OrderId} to the Orders Table", request.orderId);
        var orderDm = new OrderDataModel
        {
            OrderId = request.orderId,
            ProductId = request.productId,
            OrderQty = request.orderQty
        };

        _orderRepository.Orders.Add(orderDm);

        await _orderRepository.SaveChangesAsync(cancellationToken);

        var orderCreatedEvent = new OrderCreatedEvent(orderDm.OrderId, orderDm.ProductId, orderDm.OrderQty, DateTime.UtcNow);

        _logger.LogInformation("From CommandHandler into the Saga");
        await _createOrderSaga.HandleOrderCreatedEvent(orderCreatedEvent, cancellationToken);

        return orderCreatedEvent;
    }
}