using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Events;
using Shared.Kafka;

namespace PaymentService.Consumers;

public class TicketCreatedConsumer : KafkaConsumerService<TicketCreatedEvent>
{
    private readonly ILogger<TicketCreatedConsumer> _logger;
    private readonly KafkaProducer _kafkaProducer;

    public TicketCreatedConsumer(
        IConfiguration configuration,
        ILogger<TicketCreatedConsumer> logger,
        KafkaProducer kafkaProducer)
        : base(
            configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            "payment-service-group",
            "ticket-created",
            logger)
    {
        _logger = logger;
        _kafkaProducer = kafkaProducer;
    }

    protected override async Task ProcessMessage(TicketCreatedEvent message)
    {
        _logger.LogInformation("Bilet oluşturuldu event'i alındı: TicketId={TicketId}, Price={Price}", 
            message.TicketId, message.Price);

        // Ödeme işlemi simülasyonu
        await Task.Delay(2000); 

        var random = new Random();
        var isSuccess = random.Next(100) < 95;

        var paymentCompletedEvent = new PaymentCompletedEvent
        {
            PaymentId = random.Next(1000, 9999),
            TicketId = message.TicketId,
            UserId = message.UserId,
            Amount = message.Price,
            Status = isSuccess ? "Success" : "Failed",
            CompletedAt = DateTime.UtcNow
        };

        await _kafkaProducer.ProduceAsync("payment-completed", message.TicketId.ToString(), paymentCompletedEvent);

        _logger.LogInformation("Ödeme tamamlandı: PaymentId={PaymentId}, Status={Status}", 
            paymentCompletedEvent.PaymentId, paymentCompletedEvent.Status);
    }
}

