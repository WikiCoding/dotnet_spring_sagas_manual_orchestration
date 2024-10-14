using Microsoft.EntityFrameworkCore;
using WindowShopper.Orders.Queries.Infrastructure;
using WindowShopper.Orders.Queries.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));
builder.Services.AddHostedService<OrderCreatedKafkaConsumer>();
builder.Services.AddHostedService<OrderCancelledKafkaConsumer>();
builder.Services.AddHostedService<OrderCompletedKafkaConsumer>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

    dbContext.Database.EnsureDeleted();
    dbContext.Database.Migrate();
    dbContext.Database.EnsureCreated();
}

app.MapControllers();

app.UseHttpsRedirection();

app.Run();
