using MediatR;

namespace WindowShopper.Orders.Commands.Features.Events;

public record OrderCreatedEvent(Guid orderId, int productId, int orderQty, DateTime timestamp) : INotification;