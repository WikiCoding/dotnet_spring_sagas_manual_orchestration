using System.Text.Json;
using Confluent.Kafka;
using Microsoft.EntityFrameworkCore;
using WindowShopper.Orders.Queries.Features.Events;
using WindowShopper.Orders.Queries.Repository;

namespace WindowShopper.Orders.Queries.Infrastructure;

public class OrderCompletedKafkaConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;

    public OrderCompletedKafkaConsumer(IConfiguration configuration, IServiceProvider serviceProvider)
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
    }

    private async Task Consume(CancellationToken cancellationToken)
    {
        const string topic = "inventory-deducted-topic";
        _consumer.Subscribe(topic);

        var msg = _consumer.Consume(cancellationToken);

        if (msg == null) return;
        
        var msgValue = msg.Message.Value;

        Console.WriteLine("received completed message: " + msgValue);
        
        var orderConfirmedEvent = JsonSerializer.Deserialize<OrderConfirmedEvent>(msgValue);
        
        if (orderConfirmedEvent == null) return;

        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();

        await dbContext.Orders.Where(order => order.OrderId == orderConfirmedEvent.orderId)
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