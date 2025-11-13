# 🚀 Evently - Başlangıç Kılavuzu

## 📋 Gereksinimler

- .NET 9.0 SDK
- Docker Desktop
- PostgreSQL (veya Docker ile)
- MongoDB (veya Docker ile)

## 🏃‍♂️ Hızlı Başlangıç

### Yerel Ortamda Çalıştırma

#### 1. Veritabanlarını Başlat
```bash
# PostgreSQL
docker run -d -p 5432:5432 -e POSTGRES_USER=admin -e POSTGRES_PASSWORD=admin postgres:15

# MongoDB
docker run -d -p 27017:27017 mongo:6
```

#### 2. Servisleri Çalıştır

Her servis için ayrı terminal penceresi açın:

```bash
# Identity Service (Port 5001)
cd src/IdentityService
dotnet run

# Event Service (Port 5002)
cd src/EventService
dotnet run

# Ticket Service (Port 5003)
cd src/TicketService
dotnet run

# Payment Service (Port 5004)
cd src/PaymentService
dotnet run

# Notification Service (Port 5005)
cd src/NotificationService
dotnet run

# API Gateway (Port 5000)
cd src/ApiGateway
dotnet run
```

### Docker ile Çalıştırma

```bash
# Tüm servisleri tek komutla başlat
docker-compose up -d

# Logları izle
docker-compose logs -f

# Servisleri durdur
docker-compose down
```

## 🧪 API Test Etme

### 1. Kullanıcı Kaydı
```bash
curl -X POST http://localhost:5000/identity/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123",
    "firstName": "Test",
    "lastName": "User"
  }'
```

### 2. Giriş Yapma
```bash
curl -X POST http://localhost:5000/identity/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "test@example.com",
    "password": "Test123"
  }'
```

### 3. Etkinlik Oluşturma
```bash
curl -X POST http://localhost:5000/events \
  -H "Content-Type: application/json" \
  -H "X-User-Id: <user-id>" \
  -d '{
    "title": "Konser",
    "description": "Harika bir konser",
    "location": "İstanbul",
    "startDate": "2025-12-01T20:00:00Z",
    "endDate": "2025-12-01T23:00:00Z",
    "totalTickets": 100,
    "price": 150.00,
    "category": "Müzik"
  }'
```

### 4. Bilet Satın Alma
```bash
curl -X POST http://localhost:5000/tickets/purchase \
  -H "Content-Type: application/json" \
  -H "X-User-Id: <user-id>" \
  -d '{
    "eventId": "<event-id>",
    "price": 150.00
  }'
```

## 📊 Swagger UI

Her servis kendi Swagger UI'ına sahip:

- Identity Service: http://localhost:5001/swagger
- Event Service: http://localhost:5002/swagger
- Ticket Service: http://localhost:5003/swagger
- Payment Service: http://localhost:5004/swagger
- Notification Service: http://localhost:5005/swagger

## 🏗️ Proje Yapısı

```
Evently/
├── src/
│   ├── IdentityService/     # JWT Auth & Kullanıcı Yönetimi
│   ├── EventService/        # MongoDB ile Etkinlik CRUD
│   ├── TicketService/       # PostgreSQL ile Bilet & QR Kod
│   ├── PaymentService/      # Ödeme Simülasyonu
│   ├── NotificationService/ # Bildirim Sistemi
│   └── ApiGateway/          # Ocelot API Gateway
├── docker-compose.yml
└── README.md
```

## 🔍 Servis Portları

| Servis | Port | Açıklama |
|--------|------|----------|
| API Gateway | 5000 | Tek giriş noktası |
| Identity Service | 5001 | Kimlik doğrulama |
| Event Service | 5002 | Etkinlik yönetimi |
| Ticket Service | 5003 | Bilet yönetimi |
| Payment Service | 5004 | Ödeme işlemleri |
| Notification Service | 5005 | Bildirimler |
| PostgreSQL | 5432 | Veritabanı |
| MongoDB | 27017 | Veritabanı |

## 🛠️ Geliştirme

### Migration Oluşturma

```bash
# Identity Service
cd src/IdentityService
dotnet ef migrations add MigrationName
dotnet ef database update

# Ticket Service
cd src/TicketService
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Solution Build

```bash
dotnet build Evently.sln
```

## 🐛 Sorun Giderme

### PostgreSQL bağlantı hatası
- PostgreSQL'in çalıştığından emin olun
- Connection string'i kontrol edin

### MongoDB bağlantı hatası
- MongoDB'nin çalıştığından emin olun
- Port 27017'nin açık olduğundan emin olun

### Docker hataları
- `docker-compose down -v` ile tüm container ve volume'leri temizleyin
- Tekrar `docker-compose up -d` çalıştırın

## 📝 Notlar

- Tüm servisler otomatik migration yapar (Identity ve Ticket servisleri)
- API Gateway üzerinden tüm servislere erişilebilir
- JWT token 7 gün geçerlidir
- QR kodlar PNG formatında saklanır

