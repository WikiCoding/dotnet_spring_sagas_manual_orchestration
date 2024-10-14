using Confluent.Kafka;
using System.Text.Json;
using WindowShopper.Orders.Commands.Configuration;
using WindowShopper.Orders.Commands.Features.CreateOrder.CreateOrderSaga;
using WindowShopper.Orders.Commands.Features.Events;
using WindowShopper.Orders.Commands.Infrastructure.Messaging.Abstractions;

namespace WindowShopper.Orders.Commands.Infrastructure.Messaging.Consumers;

public class OrderCompletedConsumer : BackgroundService, IConsumerMessageBus<OrderConfirmedEvent>
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly KafkaConfig _kafkaConfig;
    private readonly IServiceProvider _serviceProvider;

    public OrderCompletedConsumer(KafkaConfig kafkaConfig, IServiceProvider serviceProvider)
    {
        _kafkaConfig = kafkaConfig;

        var consumerConfig = new ConsumerConfig
        {
            GroupId = "Window-Shopper-Group-Id",
            BootstrapServers = _kafkaConfig.BootstrapServers,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _serviceProvider = serviceProvider;
    }

    public OrderConfirmedEvent? Consume(CancellationToken cancellationToken)
    {
        const string topic = "inventory-deducted-topic";
        _consumer.Subscribe(topic);

        var msg = _consumer.Consume(cancellationToken);

        if (msg == null) return null;
        
        var msgValue = msg.Message.Value;

        Console.WriteLine("received message: " + msgValue);

        return JsonSerializer.Deserialize<OrderConfirmedEvent>(msgValue)!;

    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            var orderConfirmedEvent = Consume(stoppingToken);

            if (orderConfirmedEvent != null)
            {
                using var scope = _serviceProvider.CreateScope();
                var saga = scope.ServiceProvider.GetRequiredService<ICreateOrderSaga>();

                await saga.HandleOrderConfirmedEvent(orderConfirmedEvent, stoppingToken);
            }

            await Task.Delay(TimeSpan.FromSeconds(6), stoppingToken);
        }
        
        _consumer.Close();
    }
}