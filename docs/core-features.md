# Evently Temel Özellikler Planı

Bu doküman Evently platformuna eklenecek temel kullanıcı özelliklerinin veri modellerini, servis sınırlarını ve entegrasyon noktalarını detaylandırır. Amaç; tüm domain ekiplerinin aynı şema, API ve mesajlaşma sözleşmeleri üzerinde hizalanmasıdır.

## 1. Yorum & Değerlendirme Sistemi

### Veri Modeli
- `EventFeedback`: `Id`, `EventId`, `UserId`, `Rating (1-5)`, `Comment`, `Status (Pending|Approved|Rejected)`, `CreatedAt`.
- `EventRatingSummary`: `EventId`, `AverageRating`, `ReviewCount`, `LastCalculatedAt`.
- Statüler moderasyon servisi tarafından güncellenir; `Status` değiştikçe mesaj tetiklenir.

### FeedbackController Taslağı (`src/EventService/Controllers/FeedbackController`)
- `POST /events/{eventId}/feedback` → kullanıcı yorum oluşturur, `Pending` kaydedilir.
- `GET /events/{eventId}/feedback` → onaylı yorumlar için sayfalı liste (`page`, `pageSize`, `sort=recent|rating`).
- `GET /events/{eventId}/feedback/{feedbackId}` → tek kayıt detayını döner.
- `PUT /feedback/{feedbackId}` → yorum içeriğini veya puanını günceller (sadece sahip tarafından, `Status=Pending` ise).
- `PATCH /feedback/{feedbackId}/status` → moderatör `Status` ve opsiyonel `ModeratorNote` belirler.
- `DELETE /feedback/{feedbackId}` → içerik sahibi veya admin tarafından kaldırılır.
- `GET /events/{eventId}/feedback/average` → `EventRatingSummary` döner; boşsa anlık hesaplanır.

### `IFeedbackRepository` Taslağı
- `Task<EventFeedback> CreateAsync(EventFeedback feedback)`
- `Task<EventFeedback?> GetAsync(Guid id)`
- `Task<Paginated<EventFeedback>> ListByEventAsync(Guid eventId, FeedbackFilter filter)`
- `Task UpdateAsync(EventFeedback feedback)` (içerik düzenleme)
- `Task UpdateStatusAsync(Guid id, FeedbackStatus status, Guid moderatorId)`
- `Task DeleteAsync(Guid id)`
- `Task<EventRatingSummary> GetSummaryAsync(Guid eventId)`
- `Task UpsertSummaryAsync(EventRatingSummary summary)`
- Depolama: PostgreSQL tablo + `event_feedback_status_idx`, ayrıca ortalama puan için materialized view veya Redis cache.

### Ortalama Puan Hesaplama
1. Yeni yorum `Pending` iken ortalama değişmez.
2. `Status=Approved` olduğunda `FeedbackAggregationHandler` ortalamayı yeniden hesaplar ve `feedback-events` topic’ine `FeedbackApproved` mesajı yollar.
3. `FeedbackUpdated` veya `FeedbackDeleted` durumunda summary tekrar güncellenir.

### Bildirim & Moderasyon Akışı (`src/NotificationService`)
- `feedback-events` Kafka topic şemaları:
  - `FeedbackApproved` → etkinlik sahibine teşekkür e-postası, yorum sahibine onay bildirimi.
  - `FeedbackRejected` → kullanıcıya ret gerekçesi + tekrar gönderme linki.
  - `FeedbackReplied` → organizatör yanıtı için e-posta/SMS.
- Moderasyon SLA: 24 saat boyunca `Pending` kalan kayıtlar için `NotificationService` dashboard / e-posta uyarısı.

## 2. Favori / İstek Listesi

### Veri Modeli
- `UserFavorites`: `Id`, `UserId`, `EventId`, `Labels[]`, `ReminderOffsetMinutes (nullable)`, `Notifications (Email|SMS|Push)`, `CreatedAt`, `UpdatedAt`.
- `UNIQUE(UserId, EventId)` kısıtı aynı etkinliğin tekrar eklenmesini engeller.

### EventService API’ları
- `POST /users/me/favorites` → yeni kayıt, mevcutsa 200 + body döner.
- `DELETE /users/me/favorites/{eventId}` → ilişkiyi kaldırır.
- `GET /users/me/favorites?page=&pageSize=` → etkinlik meta bilgisiyle döner.
- `PATCH /users/me/favorites/{eventId}` → `Labels`, `ReminderOffsetMinutes` veya kanal ayarlarını günceller.
- Her eylem `favorite-events` topic’ine (`FavoriteAdded`, `FavoriteRemoved`, `FavoriteUpdated`) yazılır.

### Hatırlatma Planı (`src/NotificationService`)
- Favoriye eklenen etkinlik başlangıç zamanından `ReminderOffsetMinutes` önce Hangfire/Quartz job’ı tetiklenir.
- Job payload’ı `feedback-events` benzeri `favorite-reminders` kuyruğuna düşer, kanal bazlı şablon seçilir.
- Opsiyonel bundling: aynı gün içinde birden fazla favori varsa tek e-posta içinde liste gönderilir.

## 3. Arama & Filtreleme

### Endpoint
- `GET /events/search`
  - Parametreler: `q`, `categoryIds[]`, `startDate`, `endDate`, `minPrice`, `maxPrice`, `city`, `lat`, `lon`, `radiusKm`, `page (default 1)`, `pageSize (default 20, max 100)`, `sort=popularity|date|price_asc|price_desc`.
  - Yanıt: `items[]`, `total`, `page`, `pageSize`, `facets { categories, cities, priceRange }`.
- `GET /events/search/filters` → kategori, şehir, fiyat bantları, tarih aralık presetlerini döner.

### Arama Motoru Seçimi
- Başlangıçta PostgreSQL `tsvector` + GIN index (`events_search_idx`).
- Trafik arttığında aynı yayın akışıyla ElasticSearch `events-v1` index’i beslenir.
- CDC modeli: EventService write path’i Kafka `event-catalog` topic’ine yazar, hem Postgres hem Elastic tüketicileri indeksleri günceller.
- Elastic mapping: `name` ve `description` için `text` + `keyword`, `categoryIds` için `keyword`, `price` için `double`, `eventDate` için `date`, `location` için `geo_point`.
- Pagination: search motorunun `from/size` limitini aşmamak için `search_after` planlanır.

## 4. Etkinlik Takvimi

### Veri Modeli
- `CalendarEntry`: `Id`, `UserId`, `EventId (nullable)`, `Title`, `StartUtc`, `EndUtc`, `Source (Favorite|Ticket|Manual)`, `ReminderMinutesBefore`, `SyncStatus (Pending|Synced|Failed)`, `CreatedAt`.

### `CalendarController` (`src/EventService/Controllers/CalendarController`)
- `GET /users/me/calendar?rangeStart=&rangeEnd=` → kaynak birleşik liste.
- `POST /users/me/calendar` → manuel kayıt (zorunlu alanlar: `Title`, `StartUtc`).
- `DELETE /users/me/calendar/{entryId}` → kayıt siler.
- `GET /users/me/calendar/export.ics?rangeStart=&rangeEnd=` → ICS dosyası; backend `IcsBuilder` bileşeni üretir.
- Bilet satın alma ve favori ekleme akışları `CalendarEntry` oluşturmak için domain event yayınlar (`calendar-events` topic).

### Hatırlatıcı Scheduling
- `NotificationService` Hangfire job’ları `ReminderMinutesBefore` değerine göre planlanır, kuyruk gecikmesini azaltmak için Redis tabanlı delayed queue kullanılır.
- Dışa aktarım sonrası Google/Apple Calendar senkronizasyonu için OAuth token saklayacak `CalendarSyncService` stub’ı eklenir.

## 5. QR Kod Okuma

### QR Payload Formatı
```
{
  "ticketId": "GUID",
  "eventId": "GUID",
  "userId": "GUID",
  "seatCode": "SECTION-ROW-SEAT",
  "issuedAt": 1732798000,
  "signature": "HMACSHA256(payload, QR_SIGNING_KEY)"
}
```
- Payload base64-url kodlanır, 60 saniyeden eski taramalarda uyarı verilir.

### Doğrulama Endpoint’i (`src/TicketService`)
- `POST /tickets/qr/verify`
  1. İmza doğrulanır.
  2. Ticket durumu (`Valid|CheckedIn|Revoked`) kontrol edilir.
  3. Opsiyonel `seatCode` eşleşmesi ve cihaz `deviceId` loglanır.
  4. Başarılı ise `ticket-checkins` topic’ine `CheckInCompleted` mesajı, ayrıca `NotificationService` anlık onay e-postası.

### `apps/web` QrScanner Gereksinimleri
- `apps/web/src/components/QrScanner` bileşeni `@zxing/browser` kullanarak kamera akışını yönetir.
- Özellikler: otomatik kamera seçimi, manuel kamera değiştirme, düşük ışık modu uyarıları, tarama başarısız olduğunda fallback manuel kod girme alanı.
- Mobil tarayıcılar için `BarcodeDetector` API desteklenirse native fallback; aksi halde WebRTC akışı.

## 6. Koltuk Seçimi

### Veri Modelleri
- `SeatingPlan`: `Id`, `EventId`, `Version`, `Sections[]` (her biri `Name`, `Rows`, `Columns`, `Category`, `Price`), `AccessibilityTags[]`.
- `SeatLock`: `SeatId`, `UserId`, `LockToken`, `ExpiresAt`, `Status (Held|Committed|Expired)`, `CreatedAt`.

### Akış
1. UI `GET /events/{eventId}/seating-plan` ile planı alır.
2. Kullanıcı koltuk seçince `POST /tickets/locks` → 5 dk TTL, Redis veya PostgreSQL advisory lock.
3. Ödeme tamamlandığında `SeatLock` `Committed` olur, Kafka `seat-updates` topic’ine `SeatCommitted` mesajı yayınlanır.
4. Süresi dolan kilitler `SeatLockSweeper` job’ı ile `Expired` olur ve `seat-updates` üzerinden UI’ya itilir.

### UI Gereksinimleri
- SVG/Canvas tabanlı etkileşimli grid, renk legend’i (uygun, rezerve, satıldı, engelli erişim).
- Mobilde pinch-to-zoom ve tek dokunuşla seçim, masaüstünde çoklu seçim.
- Gerçek zamanlı koltuk durumu için WebSocket aboneliği (NotificationService).

## 7. Bilet İptal / İade

### Politika Kuralları
- Etkinlikten `X` saat öncesine kadar ücretsiz iptal; sonrası organizatör onayına bağlı, `LateCancellationFee` uygulanabilir.
- Promosyon veya partner kampanya biletlerinde sadece kupon iadesi yapılabilir.
- Organizasyon iptallerinde tam iade + servis ücreti.

### Akış & PaymentService Entegrasyonu
1. Kullanıcı `POST /tickets/{ticketId}/cancel` + `reason` gönderir.
2. `TicketService` kural uygunsa `CancellationPending` durumuna çeker ve `ticket-cancellations` topic’ine mesaj yollar.
3. `PaymentService` `RefundCalculator` ile iade tutarını belirler (`netAmount - nonRefundableFees`).
4. Ödeme sağlayıcısı (Stripe/Adyen) üzerinden `refundId` oluşturulur, statü `RefundSucceeded|RefundFailed` olarak `payment-events` topic’inde paylaşılır.
5. Ticket durumu `Refunded|RefundDeclined` olarak güncellenir, NotificationService ilgili şablonu tetikler.

### Bildirimler
- `ticket-cancel-received` → isteğin alındığını doğrular.
- `ticket-refund-approved` / `ticket-refund-declined` → e-posta/SMS.
- Organizasyona yönlendirilen iptal talepleri için admin panel uyarısı.

## 8. İndirim Kuponları

### Veri Modeli (`src/PaymentService`)
- `PromoCode`: `Id`, `Code`, `Type (Percentage|Fixed|BOGO)`, `Value`, `Currency`, `UsageLimit`, `PerUserLimit`, `ValidFrom`, `ValidUntil`, `AppliesTo (Global|Event|Organizer)`, `MinOrderAmount`, `Status (Draft|Active|Inactive|Expired)`, `CreatedBy`.
- `PromoUsage`: `Id`, `PromoId`, `UserId`, `OrderId`, `UsedAt`, `DiscountAmount`, `Channel (Web|Mobile)`.

### Doğrulama Akışı
1. Checkout sırasında `POST /payments/promo/validate` çağrısı yapılır; request `code`, `cartTotal`, `eventId`, `userId` içerir.
2. Response: `isValid`, `discountAmount`, `currency`, `reasonCode`.
3. Sipariş tamamlandığında `PromoUsage` oluşturulur; concurrency için `UsageCounter` tablosu `SELECT ... FOR UPDATE` ile güncellenir.
4. Limitler dolduğunda `PromoCode` `Inactive` olur ve `promo-events` topic’i üzerinden yöneticilere bildirilir.

### Loglama & Raporlama
- Her doğrulama isteği `promo_validation_logs` tablosuna (valid/invalid, IP, cihaz) yazılır.
- Günlük job (`PromoReportJob`) `docs/reports/promo-YYYYMMDD.csv` üretir; hata ve fraud örüntülerini Grafana dashboard’ına gönderir.
- Şüpheli aktivitelerde NotificationService Slack/Webhook bildirimi gönderir.

## 9. Katılımcı Sayacı

### Yayın Akışı
- `TicketService` `ticket-purchases` ve `ticket-checkins` olaylarını üretir.
- `EventService.EventMetricsAggregator` bu olayları tüketip Redis’de `event:{eventId}:attendees` anahtarını günceller (`purchased`, `checkedIn`).
- `NotificationService` WebSocket/SSE kanalı üzerinden istemcilere `AttendeeCountUpdated` payload’ı yayınlar.

### Ölçeklenebilirlik & Cache Stratejisi
- Redis Cluster + TTL (etkinlik bitişinden 24 saat sonra otomatik silinir).
- Evrensel counter snapshot’ları saatlik olarak Postgres/ClickHouse analitik tablosuna dump edilir.
- Önyüz istemcileri SSE akışı düşerse fallback olarak `GET /events/{id}/attendees/live` endpoint’ini çağırır (Redis okuması).

## 10. Email / SMS Bildirimleri

### Servis Entegrasyonları (`src/NotificationService`)
- SMTP: SendGrid veya Amazon SES; `NOTIFICATION_SMTP_HOST`, `PORT`, `USERNAME`, `PASSWORD`, `FROM_ADDRESS` environment değişkenleri.
- SMS: Twilio REST API (`TWILIO_ACCOUNT_SID`, `TWILIO_AUTH_TOKEN`, `TWILIO_FROM`).
- Kanal seçimi `NotificationPreference` tablosuna göre yapılır; başarısız SMS gönderimleri 3 kez retry + Dead Letter Queue.

### Şablon Matrisi
| Özellik | Şablon Adı | Kanal | Tetikleyici Olay |
| --- | --- | --- | --- |
| Yorum Onay | `feedback-approved` | Email | `FeedbackApproved` |
| Yorum Ret | `feedback-rejected` | Email | `FeedbackRejected` |
| Yorum Yanıtı | `feedback-replied` | Email/SMS | `FeedbackReplied` |
| Favori Hatırlatma | `favorite-reminder` | Email/SMS | Favori başlangıç job’u |
| Takvim Hatırlatma | `calendar-reminder` | Email/SMS | `ReminderMinutesBefore` job |
| QR Check-in | `ticket-checkin-confirmed` | Email | `CheckInCompleted` |
| Koltuk Kilidi Süresi Doldu | `seat-lock-expired` | Email/SMS | `SeatLockExpired` |
| İptal Talebi | `ticket-cancel-received` | Email | Cancel endpoint |
| İade Onayı | `ticket-refund-approved` | Email/SMS | `RefundSucceeded` |
| İade Reddedildi | `ticket-refund-declined` | Email/SMS | `RefundFailed` |
| Promo Kullanımı | `promo-confirmation` | Email | Başarılı ödeme |
| Katılımcı Milestone | `attendance-milestone` | Email | Belirli eşiklere ulaşıldığında |

### Operasyonel Notlar
- Şablonlar JSON+Handlebars formatında versiyonlu saklanır; her yayın `traceId` içerir.
- Retries için Kafka DLQ + manuel replay konsolu.
- Observability: tüm bildirimler OpenTelemetry trace’i yayınlar, Grafana/Loki dashboard’larına bağlanır.

---

Son güncelleme: `2025-11-28`
