# Evently API Gateway

API Gateway, tüm Evently mikroservislerine tek bir giriş noktası sağlar.

## ✨ Özellikler

### 1. **JWT Authentication**
- Bearer token tabanlı kimlik doğrulama
- IdentityService ile entegre
- Token validation ve yönetimi

### 2. **Rate Limiting**
- Dakikada 100 istek limiti
- IP/User bazlı kısıtlama
- Queue mekanizması (10 kuyruk)
- 429 (Too Many Requests) yanıtı

### 3. **Swagger Documentation**
- Tüm mikroservislerin API dokümantasyonu
- Tek bir yerden tüm endpoint'lere erişim
- JWT token test desteği

### 4. **Health Checks**
- Gateway kendi health check'i
- Tüm mikroservislerin health durumu
- Monitoring ve alerting için hazır

## 🚀 Çalıştırma

```bash
cd src/ApiGateway
dotnet run
```

Gateway şu adreste çalışacak: http://localhost:5000

## 📡 Endpoint'ler

### Ana Endpoint'ler
- `GET /` - Gateway bilgileri
- `GET /health` - Gateway health check
- `GET /swagger` - API dokümantasyonu

### Mikroservis Route'ları

#### Identity Service (Port 5001)
- `POST /identity/auth/register` - Kullanıcı kaydı
- `POST /identity/auth/login` - Giriş yap
- `GET /identity/health` - Health check

#### Event Service (Port 5002)
- `GET /events` - Tüm etkinlikler
- `GET /events/{id}` - Etkinlik detayı
- `POST /events` - Yeni etkinlik (Auth gerekli)
- `PUT /events/{id}` - Etkinlik güncelle (Auth gerekli)
- `DELETE /events/{id}` - Etkinlik sil (Auth gerekli)
- `GET /events/health` - Health check

#### Ticket Service (Port 5003)
- `GET /tickets` - Biletlerim (Auth gerekli)
- `POST /tickets` - Bilet satın al (Auth gerekli)
- `GET /tickets/{id}` - Bilet detayı (Auth gerekli)
- `GET /tickets/health` - Health check

#### Payment Service (Port 5004)
- `GET /payments` - Ödeme geçmişi (Auth gerekli)
- `POST /payments` - Ödeme yap (Auth gerekli)
- `GET /payments/{id}` - Ödeme detayı (Auth gerekli)
- `GET /payments/health` - Health check

#### Notification Service (Port 5005)
- `GET /notifications` - Bildirimlerim (Auth gerekli)
- `GET /notifications/health` - Health check

## 🔐 Authentication Kullanımı

### 1. Kullanıcı Kaydı
```bash
curl -X POST http://localhost:5000/identity/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!",
    "firstName": "John",
    "lastName": "Doe"
  }'
```

### 2. Login
```bash
curl -X POST http://localhost:5000/identity/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "Password123!"
  }'
```

Yanıt:
```json
{
  "token": "eyJhbGciOiJIUzI1NiIs...",
  "expiration": "2025-11-29T00:00:00Z"
}
```

### 3. Token ile İstek
```bash
curl -X GET http://localhost:5000/events \
  -H "Authorization: Bearer eyJhbGciOiJIUzI1NiIs..."
```

## ⚙️ Konfigürasyon

### appsettings.json

```json
{
  "JwtSettings": {
    "SecretKey": "EventlySecretKeyForJwtTokenGeneration12345678",
    "Issuer": "EventlyIdentityService",
    "Audience": "EventlyApiGateway",
    "ExpiryInMinutes": 60
  },
  "RateLimiting": {
    "PermitLimit": 100,
    "WindowInMinutes": 1,
    "QueueLimit": 10
  }
}
```

### ocelot.json
- Local development için (localhost:5001-5005)

### ocelot.Docker.json
- Docker ortamı için (service-name:8080)

## 🔥 Rate Limiting

Gateway, her kullanıcı/IP için dakikada 100 istek limiti koyar:

```
Window: 1 dakika
Limit: 100 istek
Queue: 10 istek kuyrukta bekleyebilir
```

Limit aşıldığında:
- HTTP 429 (Too Many Requests)
- Mesaj: "Too many requests. Please try again later."

## 📊 Health Monitoring

### Gateway Health
```bash
curl http://localhost:5000/health
```

### Tüm Servisler
```bash
curl http://localhost:5000/identity/health
curl http://localhost:5000/events/health
curl http://localhost:5000/tickets/health
curl http://localhost:5000/payments/health
curl http://localhost:5000/notifications/health
```

## 🐳 Docker

```bash
docker build -t evently-gateway .
docker run -p 5000:8080 evently-gateway
```

## 📝 Test (.http dosyası)

`ApiGateway.http` dosyasını kullanarak VS Code REST Client veya Rider ile test edebilirsiniz:

1. Kullanıcı kaydı yap
2. Login ol ve token'ı kopyala
3. Token'ı `@Token` değişkenine yapıştır
4. Authenticated endpoint'leri test et

## 🛡️ Güvenlik

- ✅ JWT token validation
- ✅ Rate limiting
- ✅ CORS yapılandırması
- ✅ HTTPS desteği (production)
- ✅ Sensitive data logging filtreleme

## 📦 Bağımlılıklar

- Ocelot 24.0.1 - API Gateway framework
- MMLib.SwaggerForOcelot 8.3.0 - Swagger aggregation
- Microsoft.AspNetCore.Authentication.JwtBearer 8.0.0 - JWT auth
- Serilog - Structured logging
- Swashbuckle - Swagger/OpenAPI

## 🔗 Faydalı Linkler

- Swagger UI: http://localhost:5000/swagger
- Gateway Info: http://localhost:5000/
- Health Check: http://localhost:5000/health

