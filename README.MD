# 🎫 Evently – Etkinlik Yönetim ve Biletleme Sistemi

## 🎯 Proje Amacı
Evently, mikroservis mimarisiyle geliştirilen bir **etkinlik yönetim ve biletleme sistemidir**.  
Kullanıcılar etkinlik oluşturabilir, diğer kullanıcılar bilet alabilir ve sistem ödemeleri simüle edip bildirim gönderebilir.  

Bu proje **.NET 8** ile mikroservis mimarisini öğrenmek için tasarlanmıştır, ancak sonuçta ortaya çıkan ürün **gerçek bir sistem gibi çalışır**.

---

## 🧩 Mikroservis Mimarisi

| Servis | Açıklama | Teknolojiler |
|--------|-----------|--------------|
| **Identity Service** | Kullanıcı kayıt, giriş, JWT üretimi | ASP.NET Core, EF Core, PostgreSQL |
| **Event Service** | Etkinlik CRUD işlemleri | ASP.NET Core, MongoDB |
| **Ticket Service** | Bilet oluşturma, satma, QR kod üretimi | ASP.NET Core, PostgreSQL |
| **Payment Service** | Ödeme simülasyonu | ASP.NET Core, RabbitMQ |
| **Notification Service** | Mail veya SignalR bildirimleri | ASP.NET Core, Kafka |
| **API Gateway** | Servislerin tek giriş noktası | Ocelot |

---

## ⚙️ Kullanılan Teknolojiler

- .NET 8 (ASP.NET Core Web API)
- Entity Framework Core 9
- MongoDB + PostgreSQL
- Kafka
- Ocelot API Gateway
- Serilog + Seq (loglama)
- Docker + Docker Compose
- Swagger / OpenAPI
- Angular

---

## 🧱 Proje Yapısı

Evently/
│
├── src/
│ ├── IdentityService/
│ ├── EventService/
│ ├── TicketService/
│ ├── PaymentService/
│ ├── NotificationService/
│ └── ApiGateway/
│
├── docker-compose.yml
├── README.md
└── .gitignore


---

## 🧠 Öğrenme Hedefleri

- Mikroservis mimarisi tasarımı  
- Servisler arası iletişim (HTTP vs Kafka)  
- API Gateway (Ocelot) kullanımı  
- JWT Authentication  
- Docker Compose ile çoklu servis orkestrasyonu  
- Merkezi loglama (Serilog + Seq)  
- Sağlık kontrolleri ve dayanıklı servis yapısı (HealthCheck, Polly)  
- Gerçekçi domain tasarımı (bilet, etkinlik, kullanıcı ilişkileri)

---

## 🔄 Servisler Arası İletişim Akışı

1. Kullanıcı bilet satın alır → **Ticket Service** bir `TicketCreatedEvent` yayınlar (Kafka).  
2. **Payment Service** bu olayı dinler → ödeme işlemini simüle eder → `PaymentCompletedEvent` yayınlar.  
3. **Notification Service** bu olayı dinler → kullanıcıya "Biletiniz başarıyla alındı" bildirimi gönderir.  
4. **Event Service** bilet satış sayısını günceller.

Bu sayede sistem tamamen **asenkron** ve **loosely coupled** şekilde çalışır.

---

## 🗓️ Geliştirme Aşamaları

| Aşama | Hedef | Çıktı |
|--------|--------|--------|
| 1️⃣ | Solution yapısını oluştur | 6 mikroservis + API Gateway |
| 2️⃣ | Identity Service | Kullanıcı kayıt, giriş, JWT üretimi |
| 3️⃣ | Event Service | Etkinlik CRUD işlemleri |
| 4️⃣ | Ticket Service | Bilet oluşturma, Kafka entegrasyonu |
| 5️⃣ | Payment Service | Ödeme simülasyonu |
| 6️⃣ | Notification Service | Mail / SignalR bildirimi |
| 7️⃣ | Docker Compose | Servisleri tek komutla ayağa kaldırma |
| 8️⃣ | Serilog + Seq | Merkezi loglama sistemi |
| 9️⃣ | (Opsiyonel) Frontend | React veya Blazor ile arayüz oluşturma |

---

## 🐳 Docker Compose (Örnek Taslak)

```yaml
version: '3.9'

services:
  postgres:
    image: postgres:15
    environment:
      POSTGRES_USER: admin
      POSTGRES_PASSWORD: admin
      POSTGRES_DB: evently_identity
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  mongodb:
    image: mongo:6
    ports:
      - "27017:27017"
    volumes:
      - mongo_data:/data/db

  rabbitmq:
    image: rabbitmq:3-management
    ports:
      - "5672:5672"
      - "15672:15672"

  seq:
    image: datalust/seq:latest
    environment:
      ACCEPT_EULA: "Y"
    ports:
      - "5341:80"

volumes:
  postgres_data:
  mongo_data:

## 🧩 API Gateway (Ocelot) Örneği
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/api/users/{everything}",
      "DownstreamScheme": "http",
      "DownstreamHostAndPorts": [
        { "Host": "identityservice", "Port": 5001 }
      ],
      "UpstreamPathTemplate": "/identity/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST", "PUT", "DELETE" ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://localhost:5000"
  }
}

#### 🧰 Geliştirme Ortamı
Bileşen	Versiyon	Açıklama
.NET SDK	8.0+	Ana geliştirme platformu
Docker Desktop	Son sürüm	Servis orkestrasyonu
PostgreSQL	15	Identity & Ticket DB
MongoDB	6	Event DB
Kafka		Mesajlaşma altyapısı
Seq	Son sürüm	Loglama & gözlemlenebilirlik
Visual Studio / VS Code	-	IDE / Editor seçimi