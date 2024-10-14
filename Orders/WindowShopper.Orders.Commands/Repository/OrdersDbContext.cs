using Microsoft.EntityFrameworkCore;

namespace WindowShopper.Orders.Commands.Repository;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OrderDataModel> Orders { get; init; }
    public DbSet<OrderEventsDataModel> OrderEvents { get; init; }
    public DbSet<SagaDataModel> Sagas { get; init; }
}