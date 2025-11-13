using System.Text.Json;
using Confluent.Kafka;

namespace Shared.Kafka;

public class KafkaProducer : IDisposable
{
    private readonly IProducer<string, string> _producer;

    public KafkaProducer(string bootstrapServers)
    {
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers,
            Acks = Acks.Leader
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task ProduceAsync<T>(string topic, string key, T message)
    {
        var json = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<string, string>
        {
            Key = key,
            Value = json
        };

        await _producer.ProduceAsync(topic, kafkaMessage);
    }

    public void Dispose()
    {
        _producer?.Dispose();
    }
}

