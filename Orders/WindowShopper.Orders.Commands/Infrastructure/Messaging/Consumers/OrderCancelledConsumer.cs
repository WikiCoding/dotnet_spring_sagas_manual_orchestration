using Confluent.Kafka;
using System.Text.Json;
using WindowShopper.Orders.Commands.Configuration;
using WindowShopper.Orders.Commands.Features.CreateOrder.CreateOrderSaga;
using WindowShopper.Orders.Commands.Features.Events;
using WindowShopper.Orders.Commands.Infrastructure.Messaging.Abstractions;

namespace WindowShopper.Orders.Commands.Infrastructure.Messaging.Consumers;

public class OrderCancelledConsumer : BackgroundService, IConsumerMessageBus<OrderCancelledEvent>
{
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly KafkaConfig _kafkaConfig;
    private readonly IServiceProvider _serviceProvider;

    public OrderCancelledConsumer(KafkaConfig kafkaConfig, IServiceProvider serviceProvider)
    {
        _kafkaConfig = kafkaConfig;
        var consumerConfig = new ConsumerConfig
        {
            BootstrapServers = _kafkaConfig.BootstrapServers,
            GroupId = "Window-Shopper-Group-Id",
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(consumerConfig).Build();
        _serviceProvider = serviceProvider;
    }

    public OrderCancelledEvent Consume(CancellationToken cancellationToken)
    {
        const string topic = "inventory-deduction-rejected-topic";
        _consumer.Subscribe(topic);

        var msg = _consumer.Consume(cancellationToken);

        if (msg != null)
        {
            var msgValue = msg.Message.Value;

            Console.WriteLine("received message: ", msg);

            _consumer.Close();

            return JsonSerializer.Deserialize<OrderCancelledEvent>(msgValue)!;
        }

        return null;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();

        while (!stoppingToken.IsCancellationRequested)
        {
            var orderCancelled = Consume(stoppingToken);

            if (orderCancelled != null)
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var saga = scope.ServiceProvider.GetRequiredService<ICreateOrderSaga>();

                    await saga.HandleOrderCancelledEvent(orderCancelled, stoppingToken);
                }
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}
