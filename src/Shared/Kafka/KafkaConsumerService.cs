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
        try
        {
            _consumer.Subscribe(_topic);
            _logger.LogInformation("Kafka topic'e abone olundu: {Topic}", _topic);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Kafka'ya bağlanılamadı. Kafka consumer devre dışı. Topic: {Topic}", _topic);
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer.Consume(TimeSpan.FromSeconds(5));

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
            catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
            {
                _logger.LogWarning("Kafka topic henüz oluşturulmamış: {Topic}. 10 saniye bekleniyor...", _topic);
                Thread.Sleep(10000);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kafka mesaj işleme hatası");
                Thread.Sleep(5000);
            }
        }

        try
        {
            _consumer.Close();
        }
        catch
        {
            // Ignore close errors
        }
    }

    protected abstract Task ProcessMessage(T message);

    public override void Dispose()
    {
        _consumer?.Dispose();
        base.Dispose();
    }
}

