using System.Text.Json;
using Confluent.Kafka;
using WindowShopper.Orders.Queries.Features.Events;
using WindowShopper.Orders.Queries.Repository;

namespace WindowShopper.Orders.Queries.Infrastructure;

public class OrderCreatedKafkaConsumer : BackgroundService
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly IConfiguration _configuration;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<OrderCreatedKafkaConsumer> _logger;

    public OrderCreatedKafkaConsumer(IConfiguration configuration, IServiceProvider serviceProvider, ILogger<OrderCreatedKafkaConsumer> logger)
    {
        _configuration = configuration;
        _serviceProvider = serviceProvider;
        _logger = logger;
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
        const string topic = "order-created-topic";
        _logger.LogInformation("Subscribing to {topic}", topic);
        _consumer.Subscribe(topic);

        var msg = _consumer.Consume(cancellationToken);

        if (msg == null) return;
        
        var msgValue = msg.Message.Value;

        var orderCreated = JsonSerializer.Deserialize<OrderCreatedEvent>(msgValue);

        if (orderCreated == null) return;
        
        _logger.LogWarning("Received OrderCreated message for OrderId: {OrderId}", orderCreated.orderId);
        
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<OrdersDbContext>();
        
        _logger.LogWarning("Saving the new Order with id {OrderId} in the Orders Table", orderCreated.orderId);
        
        var order = new OrderDataModel
        {
            OrderId = orderCreated.orderId,
            OrderQty = orderCreated.orderQty,
            ProductId = orderCreated.productId,
            OrderStatus = OrderStatus.PENDING_CONFIRMATION,
            LastUpdatedAt = DateTime.UtcNow.ToString()
        };

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync(cancellationToken);
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