# 💰 WealthFlow - Kişisel Finans ve Varlık Yönetimi Portalı

WealthFlow, kişisel gelir-gider takibinizi yapabileceğiniz, varlıklarınızı cüzdanlar aracılığıyla yönetebileceğiniz ve finansal hedeflerinize ulaşma durumunuzu yapay zeka destekli analitik öngörülerle takip edebileceğiniz modern bir **ASP.NET Core** web uygulamasıdır.

Premium, modern ve dinamik karanlık arayüzü (glassmorphism), etkileşimli grafikleri ve entegre finansal asistanıyla zengin bir kullanıcı deneyimi sunar.

---

## ✨ Özellikler

### 📊 İnteraktif Dashboard
- **Net Varlık Özeti:** Cüzdanlarınızdaki tüm varlıkların toplamını anlık olarak izleyin.
- **Nakit Akışı Grafiği (Son 6 Ay):** Gelir ve gider trendlerinizi aylık olarak karşılaştırın.
- **Kategori & Varlık Dağılımı:** Harcamalarınızın kategorisel dağılımını ve birikimlerinizin cüzdan kırılımını pasta grafikleri ile analiz edin.
- **Akıllı Varlık Simülatörü:** Aylık birikim tutarı ve yıllık getiri oranı üzerinden gelecek projeksiyonları çizin.

### 💳 Hesap & Cüzdan Yönetimi
- Banka hesapları, nakit cüzdanlar ve kredi kartları gibi birden fazla cüzdan tanımlayın.
- Cüzdanlar arası bakiye transferi yapın ve transfer geçmişini sistem günlüğüne kaydedin.
- Her cüzdana özel renk ve simge (icon) atayarak arayüzde özelleştirilmiş 3D kartlar halinde görüntüleyin.

### 📝 İşlem Defteri (Ledger)
- Tüm gelir ve gider hareketlerini cüzdan ve kategori bazında kaydedin, düzenleyin veya silin.
- Gelişmiş filtreleme ve anlık arama (debounce) ile işlemlerinizi kolayca bulun.
- İşlem kayıtlarınızı tek tıkla **CSV** formatında dışa aktarın (Excel uyumlu Türkçe karakter desteğiyle).

### 🤖 Finansal Danışman & Asistan
- Uygulama içinde yer alan **Floating Chatbot** sayesinde finansal hedeflerinizi sorgulayın.
- *"Macbook hedefime ne zaman ulaşırım?"*, *"Bütçemi aştım mı?"*, *"Sabit giderlerim ne kadar?"* gibi sorulara anında yapay zeka analizleriyle yanıtlar alın.

### 🎯 Bütçe Planlama & Birikim Hedefleri
- Kategorilere özel aylık harcama limitleri tanımlayarak bütçe aşım alarmları alın.
- Bireysel birikim hedefleri oluşturun ve mevcut birikim hızınıza göre hedef tamamlama tarihinizi (ETA) öngörün.

### ⚖️ Gelişmiş Finansal Araçlar (Premium)
- **BES (Bireysel Emeklilik Sistemi) Simülatörü:** %30 devlet katkısı dahil 10 yıllık birikim projeksiyonu.
- **Vergi Dilimi Rehberi:** Gelir vergisi matrahına göre tahmini vergi dilimi ve vergi planlaması önerileri.
- **Borç Kapatma Stratejisti:** Snowball (Kartopu) veya Avalanche (Çığ) yöntemlerine göre borç erime analizleri.
- **FIRE (Finansal Özgürlük) Hesaplayıcı:** Enflasyondan arındırılmış aylık giderlerinize göre FIRE numaranızı ve kalan süreyi hesaplayın.

---

## 🛠️ Teknoloji Yığın (Tech Stack)

- **Backend:** .NET 10.0 (ASP.NET Core MVC, Web API)
- **Veritabanı:** SQLite (Entity Framework Core - Code First)
- **Frontend:** Vanilla HTML, CSS (Karanlık Tema & Glassmorphic Arayüz), JavaScript (AJAX, SPA Router, Bootstrap 5)
- **Grafikler:** Chart.js
- **Loglama:** Serilog (Dosya bazlı ve konsol logları)

---

## 🚀 Yerel Kurulum ve Çalıştırma

### Gereksinimler
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- SQLite (Sistemde yoksa EF Core otomatik oluşturur)

### Kurulum Adımları

1. Depoyu bilgisayarınıza klonlayın veya indirin.
2. Proje dizininde terminali açın.
3. Bağımlılıkları geri yükleyin:
   ```bash
   dotnet restore
   ```
4. Uygulamayı `Development` ortamında çalıştırın:
   ```bash
   ASPNETCORE_ENVIRONMENT=Development dotnet run --project WealthFlow.Web/WealthFlow.Web.csproj --urls "http://localhost:5005"
   ```
5. Tarayıcınızda [http://localhost:5005](http://localhost:5005) adresini ziyaret edin.

*Not: Veritabanı ilk çalıştırmada otomatik olarak oluşturulacak ve örnek kategoriler/cüzdanlar ile tohumlanacaktır (DbInitializer).*

---

## 👤 Yazar
- **Anıl Mete**
