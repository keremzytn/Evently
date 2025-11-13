using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Shared.Kafka;

public abstract class KafkaConsumerService<T> : BackgroundService
{
    private readonly IConsumer<string, string> _consumer;
    private readonly ILogger<KafkaConsumerService<T>> _logger;
    private readonly string _topic;

    protected KafkaConsumerService(
        string bootstrapServers,
        string groupId,
        string topic,
        ILogger<KafkaConsumerService<T>> logger)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = false
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
        _topic = topic;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => StartConsumerLoop(stoppingToken), stoppingToken);
    }

    private void StartConsumerLoop(CancellationToken cancellationToken)
    {
        _consumer.Subscribe(_topic);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(cancellationToken);

                if (result != null)
                {
                    var message = JsonSerializer.Deserialize<T>(result.Message.Value);
                    if (message != null)
                    {
                        ProcessMessage(message).Wait(cancellationToken);
                        _consumer.Commit(result);
                    }
                }
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka mesaj işleme hatası");
            }
        }

        _consumer.Close();
    }

    protected abstract Task ProcessMessage(T message);

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}

