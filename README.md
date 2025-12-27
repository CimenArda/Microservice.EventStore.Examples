# Event Sourcing & EventStoreDB – Öğrenme Amaçlı Örnekler

Bu repo, **Event Sourcing** ve **EventStoreDB** mantığını öğrenmek amacıyla hazırlanmış **iki adet örnek uygulama** içerir.  
Odak noktası; event üretme, stream’e yazma, subscribe olma ve event’lerden state oluşturma süreçleridir.

---

## 🏦 Uygulama 1 – Banka / Bakiye Event Sourcing Örneği

Bu uygulama, Event Sourcing’in **en temel ve saf kullanımını** göstermek için hazırlanmıştır.

### Neler Yapıldı?
- Hesap oluşturma, para yatırma, para çekme ve transfer işlemleri **event** olarak kaydedildi
- Hiçbir yerde bakiye tutulmadı
- Tüm bakiye bilgisi, stream’e yazılan event’lerin **baştan okunmasıyla** hesaplandı
- EventStoreDB stream’ine subscribe olunarak state **runtime’da** yeniden inşa edildi

### Öğrenilenler
- EventStoreDB’ye event append etme
- Stream kavramı
- Subscribe mekanizması
- Event’lerden state oluşturma (replay)

---

## 🛒 Uygulama 2 – Product Management (Event Sourcing + MongoDB)

Bu uygulama, Event Sourcing’in nasıl kullanılacağını gösterir.

### Neler Yapıldı?
- Ürün oluşturma ve güncelleme işlemleri **event** olarak EventStoreDB’ye yazıldı
- UI katmanında **MongoDB’ye doğrudan update yapılmadı**
- Background Worker, ProductStream’i dinleyerek MongoDB’de **Read Model** oluşturdu
- Ürün listesi ve detayları MongoDB üzerinden okundu

### Kullanılan Yaklaşım
- Write side → EventStoreDB
- Read side → MongoDB
- Event → Projection → Read Model akışı

### Öğrenilenler
- Event Sourcing + MVC entegrasyonu
- Write / Read ayrımı
- Projection (event handler) mantığı
- Eventually Consistent yapı

---

## 🎯 Amaç

Bu repo, Event Sourcing’i:
- teoriden çıkarıp
- çalışan kodla
- küçük ve anlaşılır örnekler üzerinden

öğrenmek için hazırlanmıştır.
