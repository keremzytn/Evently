# 🚀 Evently Servislerini Başlatma Rehberi

## Tamamlanan Özellikler ✅

- ✅ 6 Mikroservis + API Gateway
- ✅ **Kafka** entegrasyonu (Event-Driven mimari)
- ✅ **Serilog + Seq** (Merkezi loglama)
- ✅ **HealthChecks** (Tüm servislerde `/health` endpoint)
- ✅ Docker Compose yapılandırması
- ✅ PostgreSQL + MongoDB + Kafka + Seq

## Event-Driven Akış 🔄

1. **Kullanıcı bilet alır** → TicketService → Kafka'ya `ticket-created` event
2. **PaymentService** event'i dinler → Ödeme simüle eder → `payment-completed` event
3. **NotificationService** event'i dinler → Kullanıcıya bildirim gönderir

## Docker ile Başlatma

### 1️⃣ Altyapı Servislerini Başlat

```bash
docker compose up -d postgres mongodb zookeeper kafka seq
```

**Beklenen Çıktı:**
- PostgreSQL: `localhost:5432`
- MongoDB: `localhost:27017`
- Kafka: `localhost:9092`
- Seq (Loglama UI): `http://localhost:5341`

### 2️⃣ Tüm Mikroservisleri Başlat

```bash
docker compose up -d
```

**Servisler:**
- Identity Service: `http://localhost:5001`
- Event Service: `http://localhost:5002`
- Ticket Service: `http://localhost:5003`
- Payment Service: `http://localhost:5004`
- Notification Service: `http://localhost:5005`
- API Gateway: `http://localhost:5000`

### 3️⃣ Logları İzle

```bash
docker compose logs -f
```

Sadece belirli bir servisi izlemek için:

```bash
docker compose logs -f ticket-service
docker compose logs -f payment-service
docker compose logs -f notification-service
```

### 4️⃣ Health Check

```bash
curl http://localhost:5001/health
curl http://localhost:5002/health
curl http://localhost:5003/health
```

## Test Senaryosu 🧪

### 1. Kullanıcı Kaydı

```bash
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@evently.com",
    "password": "Test123",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### 2. Giriş Yap (JWT Token Al)

```bash
curl -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@evently.com",
    "password": "Test123"
  }'
```

**Response'tan `token` kopyala**

### 3. Etkinlik Oluştur

```bash
curl -X POST http://localhost:5002/api/events \
  -H "Content-Type: application/json" \
  -d '{
    "title": "Rock Konseri",
    "description": "Harika bir konser",
    "location": "İstanbul",
    "date": "2025-12-25T20:00:00Z",
    "ticketPrice": 250.00,
    "availableTickets": 100
  }'
```

**Response'tan `id` kopyala (örn: `507f1f77bcf86cd799439011`)**

### 4. Bilet Satın Al (Kafka Event Zinciri Başlar!)

```bash
curl -X POST http://localhost:5003/api/tickets/purchase \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {TOKEN}" \
  -d '{
    "eventId": 1,
    "price": 250.00
  }'
```

🎯 **Bu noktada:**
1. TicketService bilet oluşturur
2. Kafka'ya `TicketCreatedEvent` gönderir
3. PaymentService event'i yakalar, ödeme simüle eder
4. PaymentService Kafka'ya `PaymentCompletedEvent` gönderir
5. NotificationService event'i yakalar, kullanıcıya bildirim gönderir

## Seq ile Log İzleme 📊

1. Tarayıcıda aç: `http://localhost:5341`
2. Sol menüden "Events" seç
3. Filtreleme:
   - `Service = "TicketService"`
   - `Service = "PaymentService"`
   - `Service = "NotificationService"`

**Aranacak Loglar:**
- "Bilet oluşturuldu event'i alındı"
- "Ödeme tamamlandı"
- "📧 Bildirim Gönderildi"

## Servisleri Durdurma

```bash
# Tüm servisleri durdur
docker compose down

# Volume'ları da sil (DB verilerini temizle)
docker compose down -v
```

## Troubleshooting

### Kafka bağlantı hatası

```bash
# Kafka hazır mı kontrol et
docker compose logs kafka | grep "started"
```

### Seq'e log gitmiyor

```bash
# Servis ortam değişkenlerini kontrol et
docker compose ps
docker compose exec ticket-service env | grep Seq
```

### PostgreSQL bağlantı hatası

```bash
# PostgreSQL hazır mı
docker compose logs postgres | grep "ready"
```

## Geliştirme Notları

- **Kafka Consumer Group:** Her servis kendi group ID'sine sahip
- **Database Migration:** Identity ve Ticket servisleri otomatik migration yapar
- **QR Kod:** Her bilet için otomatik QR kod üretilir
- **Ödeme Simülasyonu:** %95 başarı oranıyla rastgele sonuç

## Sonraki Adımlar (Opsiyonel)

- [ ] API Gateway'e JWT doğrulama middleware ekle
- [ ] Polly ile retry/circuit breaker politikaları
- [ ] Frontend (Angular/React)
- [ ] Unit & Integration testleri
- [ ] Kubernetes deployment

