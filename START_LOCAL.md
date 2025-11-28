# Lokal Çalıştırma Kılavuzu

Bu yöntem ile sadece altyapı servisleri Docker'da, .NET servisleri ve Angular lokal çalışır. **ÇOK DAHA HIZLI!**

## 1. Altyapı Servislerini Başlat (Docker)

```bash
docker compose -f docker-compose.infrastructure.yml up -d
```

Bu şunları başlatır:
- ✅ PostgreSQL (Port 5432)
- ✅ MongoDB (Port 27017)
- ✅ Kafka (Port 9092)
- ✅ Zookeeper (Port 2181)
- ✅ Seq (Port 5341)

## 2. .NET Servislerini Başlat (6 Terminal)

### Terminal 1 - API Gateway
```bash
cd src/ApiGateway
dotnet run
```

### Terminal 2 - Identity Service
```bash
cd src/IdentityService
dotnet run
```

### Terminal 3 - Event Service
```bash
cd src/EventService
dotnet run
```

### Terminal 4 - Ticket Service
```bash
cd src/TicketService
dotnet run
```

### Terminal 5 - Payment Service
```bash
cd src/PaymentService
dotnet run
```

### Terminal 6 - Notification Service
```bash
cd src/NotificationService
dotnet run
```

## 3. Angular Client Başlat (7. Terminal)

```bash
cd src/client
npm start
# veya
ng serve
```

## Portlar

| Servis | Port | URL |
|--------|------|-----|
| API Gateway | 5000 | http://localhost:5000 |
| Identity Service | 5001 | http://localhost:5001 |
| Event Service | 5002 | http://localhost:5002 |
| Ticket Service | 5003 | http://localhost:5003 |
| Payment Service | 5004 | http://localhost:5004 |
| Notification Service | 5005 | http://localhost:5005 |
| Angular Client | 4200 | http://localhost:4200 |
| Seq (Logs) | 5341 | http://localhost:5341 |

## Durdurma

```bash
# Altyapıyı durdur
docker compose -f docker-compose.infrastructure.yml down

# .NET servisleri: Her terminalde Ctrl+C
# Angular: Ctrl+C
```

## Avantajları

- ✅ Docker build hatası yok
- ✅ Çok daha hızlı başlatma
- ✅ Hot reload çalışır (.NET ve Angular)
- ✅ Debug kolay
- ✅ Log'lar terminalde görünür

