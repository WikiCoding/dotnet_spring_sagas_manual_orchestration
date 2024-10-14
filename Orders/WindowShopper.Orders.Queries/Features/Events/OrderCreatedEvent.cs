namespace WindowShopper.Orders.Queries.Features.Events;

public record OrderCreatedEvent(Guid orderId, int productId, int orderQty, string timestamp);