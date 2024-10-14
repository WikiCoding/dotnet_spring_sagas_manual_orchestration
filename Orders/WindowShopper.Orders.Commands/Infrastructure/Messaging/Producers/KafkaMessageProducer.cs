using Confluent.Kafka;
using WindowShopper.Orders.Commands.Configuration;
using WindowShopper.Orders.Commands.Infrastructure.Messaging.Abstractions;

namespace WindowShopper.Orders.Commands.Infrastructure.Messaging.Producers;

public class KafkaMessageProducer : IProducerMessageBus
{
    private readonly IProducer<Null, string> _producer;
    private readonly KafkaConfig _kafkaConfig;
    private readonly ILogger<KafkaMessageProducer> _logger;

    public KafkaMessageProducer(KafkaConfig kafkaConfig, ILogger<KafkaMessageProducer> logger)
    {
        _kafkaConfig = kafkaConfig;
        _logger = logger;

        var producerConfig = new ProducerConfig
        {
            BootstrapServers = _kafkaConfig.BootstrapServers,
        };

        _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
    }

    public async Task ProduceAsync(string topic, string message, CancellationToken cancellationToken)
    {
        var kafkaMessage = new Message<Null, string> { Value = message };

        DeliveryResult<Null, string> result = await _producer.ProduceAsync(topic, kafkaMessage, cancellationToken);

        _logger.LogWarning("Message: {msg} successfully delivered to Kafka: ", result.Value);
    }
}