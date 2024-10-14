using System.ComponentModel.DataAnnotations;

namespace WindowShopper.Orders.Commands.Repository;

public class OrderDataModel
{
  [Key]
  public Guid OrderId { get; set; }
  public int ProductId { get; set; }
  public int OrderQty { get; set; }
  public OrderStatus OrderStatus { get; set; } = OrderStatus.PENDING_CONFIRMATION;
}