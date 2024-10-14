using Microsoft.EntityFrameworkCore;
using WindowShopper.Orders.Commands.Configuration;
using WindowShopper.Orders.Commands.Features.CreateOrder;
using WindowShopper.Orders.Commands.Features.CreateOrder.CreateOrderSaga;
using WindowShopper.Orders.Commands.Infrastructure.Messaging.Abstractions;
using WindowShopper.Orders.Commands.Infrastructure.Messaging.Consumers;
using WindowShopper.Orders.Commands.Infrastructure.Messaging.Producers;
using WindowShopper.Orders.Commands.Repository;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<OrdersDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("OrdersDb")));
var kafkaConfig = builder.Configuration.GetSection("Kafka").Get<KafkaConfig>() ?? new KafkaConfig();
builder.Services.AddMediatR(config => config.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Services.AddScoped<ICreateOrderSaga, CreateOrderSaga>();
builder.Services.AddSingleton(kafkaConfig);
builder.Services.AddScoped<IProducerMessageBus, KafkaMessageProducer>();
builder.Services.AddHostedService<OrderCompletedConsumer>();
builder.Services.AddHostedService<OrderCancelledConsumer>();
builder.Services.AddScoped<OrderCompletedConsumer>();
builder.Services.AddScoped<OrderCancelledConsumer>();

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
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        dbContext.Database.EnsureDeleted();
        dbContext.Database.EnsureCreated();
    }
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
