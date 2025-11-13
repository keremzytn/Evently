using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Shared.Events;
using Shared.Kafka;

namespace NotificationService.Consumers;

public class PaymentCompletedConsumer : KafkaConsumerService<PaymentCompletedEvent>
{
    private readonly ILogger<PaymentCompletedConsumer> _logger;

    public PaymentCompletedConsumer(
        IConfiguration configuration,
        ILogger<PaymentCompletedConsumer> logger)
        : base(
            configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            "notification-service-group",
            "payment-completed",
            logger)
    {
        _logger = logger;
    }

    protected override async Task ProcessMessage(PaymentCompletedEvent message)
    {
        _logger.LogInformation("Ödeme tamamlandı event'i alındı: PaymentId={PaymentId}, Status={Status}", 
            message.PaymentId, message.Status);

        if (message.Status == "Success")
        {
            // Bildirim gönderme simülasyonu
            await SendNotification(message.UserId, 
                $"Biletiniz başarıyla alındı! Ödeme Tutarı: {message.Amount} TL");
        }
        else
        {
            await SendNotification(message.UserId, 
                "Ödeme işlemi başarısız oldu. Lütfen tekrar deneyin.");
        }
    }

    private async Task SendNotification(string userId, string message)
    {
        // Gerçek bir mail/SMS/push notification servisi yerine loglama
        await Task.Delay(500);
        _logger.LogInformation("📧 Bildirim Gönderildi -> UserId: {UserId}, Mesaj: {Message}", 
            userId, message);
    }
}

