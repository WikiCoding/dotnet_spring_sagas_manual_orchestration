using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WindowShopper.Orders.Commands.Repository;

public class SagaDataModel
{
    [Key]
    public long EntryId { get; set; }
    public Guid CorrelationId { get; set; } = Guid.NewGuid();
    public string SagaCurrentStep { get; set; } = string.Empty;
    public Guid OrderId { get; set; }
}
