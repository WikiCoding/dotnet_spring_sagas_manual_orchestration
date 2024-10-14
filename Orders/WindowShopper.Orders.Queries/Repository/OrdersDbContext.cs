using Microsoft.EntityFrameworkCore;

namespace WindowShopper.Orders.Queries.Repository;

public class OrdersDbContext(DbContextOptions<OrdersDbContext> options) : DbContext(options)
{
    public DbSet<OrderDataModel> Orders { get; init; }
}