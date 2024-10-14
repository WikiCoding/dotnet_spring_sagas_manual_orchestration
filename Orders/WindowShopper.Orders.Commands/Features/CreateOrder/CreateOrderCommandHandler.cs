using MediatR;
using WindowShopper.Orders.Commands.Features.Commands;
using WindowShopper.Orders.Commands.Features.CreateOrder.CreateOrderSaga;
using WindowShopper.Orders.Commands.Features.Events;
using WindowShopper.Orders.Commands.Repository;

namespace WindowShopper.Orders.Commands.Features.CreateOrder;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, OrderCreatedEvent>
{
    private readonly OrdersDbContext _orderRepository;
    private readonly IMediator _mediator;
    private readonly ICreateOrderSaga _createOrderSaga;

    public CreateOrderCommandHandler(OrdersDbContext orderRepository, IMediator mediator, ICreateOrderSaga createOrderSaga)
    {
        _orderRepository = orderRepository;
        _mediator = mediator;
        _createOrderSaga = createOrderSaga;
    }

    public async Task<OrderCreatedEvent> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var orderDm = new OrderDataModel
        {
            OrderId = request.orderId,
            ProductId = request.productId,
            OrderQty = request.orderQty
        };

        _orderRepository.Orders.Add(orderDm);

        await _orderRepository.SaveChangesAsync(cancellationToken);

        var orderCreatedEvent = new OrderCreatedEvent(orderDm.OrderId, orderDm.ProductId, orderDm.OrderQty, DateTime.UtcNow);

        await _createOrderSaga.HandleOrderCreatedEvent(orderCreatedEvent, cancellationToken);

        return orderCreatedEvent;
    }
}