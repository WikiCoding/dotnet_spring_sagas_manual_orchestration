using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using WindowShopper.Orders.Queries.Features.Events;
using WindowShopper.Orders.Queries.Repository;

namespace WindowShopper.Orders.Queries.Infrastructure;

public class OrderCancelledKafkaConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderCancelledKafkaConsumer> _logger;

    public OrderCancelledKafkaConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<OrderCancelledKafkaConsumer> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _configuration["Kafka:BootstrapServers"],
            GroupId = _configuration["Kafka:GroupId"],
            AutoOffsetReset = AutoOffsetReset.Earliest
        };
        
        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _logger = logger;
    }

    private async Task Consume(CancellationToken cancellationToken)
    {
        const string topic = "inventory-deduction-rejected-topic";
        _logger.LogInformation("Subscribing to {topic}", topic);
        _consumer.Subscribe(topic);

        var msg = _consumer.Consume(cancellationToken);

        if (msg == null) return;
        
        var msgValue = msg.Message.Value;

        var orderCancelledEvent = JsonSerializer.Deserialize<OrderCancelledEvent>(msgValue);
        
        if (orderCancelledEvent == null) return;
        
        _logger.LogWarning("Received OrderCancelled message for OrderId: {OrderId}", orderCancelledEvent.orderId);

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        _logger.LogWarning("Updating the Orders Table State for OrderId: {OrderId}", orderCancelledEvent.orderId);
        
        await dbContext.Orders.Where(order => order.OrderId == orderCancelledEvent.orderId)
            .ExecuteUpdateAsync(order =>
                    order.SetProperty(o => o.OrderStatus, OrderStatus.CANCELLED)
                        .SetProperty(o => o.LastUpdatedAt, DateTime.UtcNow.ToString()), cancellationToken);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            await Consume(stoppingToken);
            
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
        
        _consumer.Close();
    }
}