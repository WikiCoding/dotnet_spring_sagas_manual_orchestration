using MediatR;
using WindowShopper.Orders.Commands.Features.Events;

namespace WindowShopper.Orders.Commands.Features.Commands;

public record CreateOrderCommand(int productId, int orderQty, Guid orderId = new Guid()) : IRequest<OrderCreatedEvent>;