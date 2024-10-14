using Microsoft.EntityFrameworkCore;

namespace WindowShopper.Orders.Commands.Repository;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OrderDataModel> Orders { get; set; }
    public DbSet<SagaDataModel> Sagas { get; set; }
}