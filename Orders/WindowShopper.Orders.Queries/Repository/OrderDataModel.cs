using System.ComponentModel.DataAnnotations;

namespace WindowShopper.Orders.Queries.Repository;

public class OrderDataModel
{
    [Key]
    public int Id { get; set; }
    public Guid OrderId { get; set; }
    public int ProductId { get; set; }
    public int OrderQty { get; set; }
    public OrderStatus OrderStatus { get; set; } = OrderStatus.PENDING_CONFIRMATION;
    public string LastUpdatedAt { get; set; }
}