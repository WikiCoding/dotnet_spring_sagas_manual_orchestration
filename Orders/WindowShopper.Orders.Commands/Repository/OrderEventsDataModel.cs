using System.ComponentModel.DataAnnotations;

namespace WindowShopper.Orders.Commands.Repository;

public class OrderEventsDataModel
{
    [Key]
    public Guid EventId { get; set; }
    public Guid OrderId { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.PENDING_CONFIRMATION;
    public string EventName { get; set; }
    public DateTime CreatedAt { get; set; }
}