# Evently Temel Özellikleri

Bu doküman Evently platformuna eklenecek çekirdek kullanıcı özelliklerinin veri modellerini, servis sınırlarını ve entegrasyon noktalarını özetler.

## 1. Yorum & Değerlendirme Sistemi

### Veri Modeli
- `EventFeedback`: `Id`, `EventId`, `UserId`, `Rating (1-5)`, `Comment`, `Status (Pending|Approved|Rejected)`, `ModeratorId`, `CreatedAt`, `UpdatedAt`.
- `EventAggregateRating`: `EventId`, `AverageRating`, `ReviewCount`, `LastRecalculatedAt`.

### Servisler ve API
- `src/EventService/Controllers/FeedbackController`: 
  - `POST /events/{id}/feedback`: yorum ekler, varsayılan durum `Pending`.
  - `GET /events/{id}/feedback`: onaylı yorumları sayfalı döndürür.
  - `GET /events/{id}/rating`: `EventAggregateRating` döner.
  - `PATCH /feedback/{id}`: moderasyon statü güncellemesi.
- `IFeedbackRepository`: Mongo koleksiyonları `eventFeedback` ve `eventAggregateRatings`.
- Ortalama puan; moderasyon onayı sonrası `feedback-events` Kafka mesajı ile `NotificationService` ve `EventService` içi `FeedbackAggregationHandler`’a yayınlanır.

### Bildirim Akışı
- `NotificationService` `feedback-events` topic’ini tüketir:
  - `FeedbackApproved` → Etkinlik sahibine e-posta.
  - `FeedbackRejected` → Kullanıcıya ret gerekçesi.
  - `FeedbackReplied` → Yorum sahibine yanıt bildirimi.

### Moderasyon ve Ölçüm
- Otomatik filtreleme için basit kötü içerik listesi + opsiyonel NLP servisi.
- `Status=Pending` kayıtlar 24 saat içinde sonuçlanmazsa SLA uyarısı; `NotificationService` geciken kayıtları haftalık raporlar.

## 2. Favori / İstek Listesi

### Veri Modeli
- `UserFavorite`: `Id`, `UserId`, `EventId`, `Pinned (bool)`, `CreatedAt`.
- `FavoriteReminder`: `FavoriteId`, `ReminderOffsetMinutes`, `NotificationChannel (Email|SMS|Push)`.

### API ve Davranış
- `POST /users/me/favorites` → mevcut değilse ekler, varsa 200 döner.
- `DELETE /users/me/favorites/{eventId}` → kaldırır.
- `GET /users/me/favorites` → etkinlik meta bilgisiyle birlikte sayfalı liste.
- `PATCH /users/me/favorites/{eventId}` → `Pinned` veya hatırlatma bilgilerini günceller.
- EventService favori eylemlerini `favorite-events` topic’ine yazar; NotificationService tetiklenir.

### Hatırlatıcılar
- `NotificationService` Hangfire/Quartz job’ı favori etkinlik başlangıcından `ReminderOffsetMinutes` önce e-posta/SMS gönderir.
- Favori listesi ile takvim girişleri senkron tutmak için `CalendarSyncHandler`.

## 3. Arama & Filtreleme

### Arama Parametreleri
- `q` (serbest metin), `categoryIds[]`, `startDate`, `endDate`, `minPrice`, `maxPrice`, `city`, `venue`, `page`, `pageSize`, `sort` (`popularity|date|price`).

### Teknik Strateji
- Depo Postgres ise `tsvector` full-text; ölçek ihtiyacı yüksek ise ElasticSearch cluster’ı (`events-index`) tutulur.
- Etkinlik kayıtları EventService tarafından `events-index`’e senkron push edilir (CDC ya da publish-on-write).
- Filtreler; kategori ve şehir için keyword fields, fiyat için numeric range, tarih için `date_histogram`.

### API
- `GET /events/search` → default `pageSize=20`, maksimum 100.
- `GET /events/filters/meta` → kullanılabilir kategoriler, şehirler, fiyat aralığı.

## 4. Etkinlik Takvimi

### Veri Modeli
- `CalendarEntry`: `Id`, `UserId`, `EventId`, `Source (Favorite|Ticket|Manual)`, `ReminderMinutesBefore`, `SyncStatus`.

### Servisler
- `CalendarController`:
  - `GET /users/me/calendar?rangeStart=&rangeEnd=` → tekleştirilmiş liste.
  - `POST /users/me/calendar` → manuel giriş.
  - `DELETE /users/me/calendar/{entryId}`.
  - `GET /users/me/calendar/export.ics` → ICS oluşturur.
- Hatırlatıcılar NotificationService job kuyruğunda saklanır; ICS üretimi için `IcsBuilder`.

### Harici Entegrasyonlar
- Google/Apple Calendar push planı: OAuth token saklama + webhook (optional future).
- Favori ve bilet satın alma akışları `CalendarEntry` ekler (`Source` alanı ile ayrıştırılır).

## 5. QR Kod Okuma

### QR Formatı
- Payload JSON: `{ "ticketId": "...", "eventId": "...", "userId": "...", "seatCode": "...", "issuedAt": epoch, "signature": "HMACSHA256" }`.
- İmzalar `TicketService` sunucu tarafı `QR_SIGNING_KEY` ile üretilir.

### Doğrulama Akışı
- `POST /tickets/verify-qr` → QR payload + tarama cihazı `deviceId`.
- Adımlar:
  1. İmza doğrula.
  2. Ticket durumu (`Valid|CheckedIn|Revoked`) kontrol.
  3. Opsiyonel `seatCode` eşleşmesi.
  4. Başarılıysa `CheckIn` eventi yayınla (`ticket-checkins` topic).

### İstemci Gereksinimleri
- Web: `apps/client/src/app/components/qr-scanner` içinde `@zxing/browser` kullanımı, kamera izin uyarıları.
- Mobil: Capacitor plugin ile yerel kamera erişimi.

## 6. Koltuk Seçimi

### Veri Modeli
- `SeatingPlan`: `Id`, `EventId`, `Sections[]` (ad, sıra, sütun, kategori, fiyat).
- `SeatLock`: `SeatId`, `UserId`, `ExpiresAt`, `Status (Held|Committed|Expired)`.

### Akış
- Event sayfası `GET /events/{id}/seating-plan` çağırır.
- Kullanıcı koltuk seçince `POST /tickets/locks` → 5 dk TTL ile kilitler, Redis veya PostgreSQL advisory lock.
- Ödeme tamamlanınca `SeatLock` `Committed` olur, Kafka `seat-updates` topic’ine yayınlanır ve UI canlı güncellenir.
- Locks job’ı periyodik olarak `Expired` durumuna çeker.

### UI Gereksinimleri
- SVG/Canvas tabanlı grid, engelli erişim ve fiyat legend’i.
- Mobilde pinch-zoom, koltuk durum legend’i (uygun, tutuldu, satıldı).

## 7. Bilet İptal / İade

### Politika
- Etkinlik başlamadan `X` saat öncesine kadar ücretsiz iptal; sonrası organizatör onayına tabi.
- Promosyon biletleri için kısmi iade veya kupon.

### Akış
1. Kullanıcı `POST /tickets/{id}/cancel` çağırır, `reason` alanı ile.
2. TicketService politika kontrolü yapar, uygun ise `CancellationPending`.
3. `ticket-cancellations` Kafka mesajı hem PaymentService hem NotificationService’e gider.
4. PaymentService iade tutarını hesaplar (`netAmount - nonRefundableFees`) ve ödeme sağlayıcısına `refundId` alır.
5. Payment sonucu `RefundSucceeded|RefundFailed` olayları yayılarak bilet durumu güncellenir.

### Bildirimler
- Duruma göre e-posta/SMS şablonları:
  - `ticket-cancel-received`
  - `ticket-refund-approved`
  - `ticket-refund-declined`

## 8. İndirim Kuponları

### Veri Modeli
- `PromoCode`: `Code`, `Type (Percentage|Fixed|BOGO)`, `Value`, `Currency`, `UsageLimit`, `PerUserLimit`, `ValidFrom`, `ValidUntil`, `AppliesTo (Global|Event|Organizer)`, `MinOrderAmount`, `Status`.
- `PromoUsage`: `PromoId`, `UserId`, `OrderId`, `UsedAt`, `DiscountAmount`.

### Kullanım Akışı
1. Checkout sırasında `POST /payments/promo/validate` ile doğrulama.
2. Dönen cevap: `isValid`, `discountAmount`, `summary`.
3. Sipariş tamamlanınca PaymentService `PromoUsage` kaydı oluşturur.
4. Limitler aşılırsa `PromoCode` `Inactive` olur; yöneticilere rapor.

### Raporlama
- Günlük/haftalık kupon performansı, `docs/core-features.md`’deki tabloyu temel alan `PromoReportJob`.
- Fraud tespiti için aynı kullanıcı kartı + IP kombinasyonlarını incele.

## 9. Katılımcı Sayacı

### Akış
- TicketService `ticket-checkins` ve `ticket-purchases` olaylarını `EventMetricsAggregator`’a gönderir.
- `EventMetricsAggregator` Redis’de `event:{id}:attendees` anahtarı tutar.
- Event sayfası WebSocket/SSE ile `NotificationService` üzerinden gerçek zamanlı güncelleme alır.

### Ölçeklenebilirlik
- Redis Cluster + TTL (etkinlik bitişinden sonra 24 saat).
- Snapshot job’ı değerleri analitik veri ambarına yazar.

## 10. Email / SMS Bildirimleri

### Servis Entegrasyonları
- SMTP: SendGrid veya Amazon SES; `NOTIFICATION_SMTP_*` konfigürasyonları.
- SMS: Twilio (`TWILIO_ACCOUNT_SID`, `TWILIO_AUTH_TOKEN`, `TWILIO_FROM`).

### Şablon Matrisi
| Özellik | Şablon | Kanal | Tetikleyici |
| --- | --- | --- | --- |
| Yorum Onaylandı | `feedback-approved` | Email | `FeedbackApproved` |
| Yorum Reddedildi | `feedback-rejected` | Email | `FeedbackRejected` |
| Favori Hatırlatma | `favorite-reminder` | Email/SMS | Favori etkinlik başlangıcı |
| Takvim Hatırlatma | `calendar-reminder` | Email/SMS | `ReminderMinutesBefore` job |
| QR Check-in | `ticket-checkin-confirmed` | Email | Başarılı tarama |
| Koltuk Kilidi Süresi Doldu | `seat-lock-expired` | Email/SMS | Lock job |
| İptal Talebi | `ticket-cancel-received` | Email | Cancel endpoint |
| İade Onayı | `ticket-refund-approved` | Email/SMS | `RefundSucceeded` |
| Promo Kullanımı | `promo-confirmation` | Email | Başarılı ödeme |

### Operasyonel Notlar
- Bildirim şablonları `NotificationService` içinde dosya tabanlı veya veritabanı tabanlı saklanır.
- Retries için `DeadLetterQueue` (Kafka) + manuel replay konsolu.
- Her mesajda `traceId` loglanır, merkezi observability (Grafana/Loki) ile izlenir.

---

Son güncelleme: `2025-11-28`

