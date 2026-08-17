# Probel — Hasta Memnuniyet Anketi Uygulaması

Sağlık Bakanlığı'nın **Sağlıkta Kalite Standartları (SKS) Anket Uygulama Rehberi**'ne uygun olarak hazırlanmış, hastanelerin hasta memnuniyet anketlerini dijital ortamda tasarlayıp, QR kod / e-posta / SMS ile dağıtıp, tamamen anonim biçimde toplayıp, standart SKS formülüyle raporlayabildiği kurumsal bir web uygulaması.

Bu proje, **Probel** bünyesinde yürütülen bir yazılım mühendisliği staj çalışması kapsamında, sıfırdan tasarlanıp geliştirilmiştir.

---

## İçindekiler

- [Projenin Amacı](#projenin-amacı)
- [Özellikler](#özellikler)
- [Kullanılan Teknolojiler](#kullanılan-teknolojiler)
- [Mimari](#mimari)
- [Veritabanı Şeması](#veritabanı-şeması)
- [Kurulum](#kurulum)
- [Kullanım](#kullanım)
- [Proje Yapısı](#proje-yapısı)
- [Tasarım / Tema](#tasarım--tema)
- [Güvenlik ve Anonimlik](#güvenlik-ve-anonimlik)
- [Test](#test)
- [Bilinen Sınırlamalar](#bilinen-sınırlamalar)
- [Yol Haritası](#yol-haritası)

---

## Projenin Amacı

Hastaneler, hasta deneyimini ölçmek ve iyileştirmek amacıyla SKS rehberi kapsamında düzenli olarak anket uygulamakla yükümlüdür. Bu süreç genellikle kağıt anketler veya dağınık Excel tabloları ile yürütülür — bu da hem veri toplamayı hem de analiz etmeyi zorlaştırır.

Bu proje, bu süreci uçtan uca dijitalleştirir:

1. Kalite birimi, sisteme anket tasarlar (SKS soru setleri veya özel sorular).
2. Anket yayınlanır, her hasta için tek kullanımlık bir QR kod / SMS / e-posta bağlantısı üretilir.
3. Hasta, kimliğini hiç paylaşmadan, telefonundan anketi doldurur.
4. Sistem, SKS'nin resmi ağırlıklandırma formülüyle otomatik olarak "karşılanma oranı" hesaplar.
5. Düşük puan alan konular otomatik işaretlenir, kalite birimi bunlar için aksiyon planı oluşturabilir.

---

## Özellikler

### Anket Yönetimi
- Anket oluşturma, bölüm ve soru ekleme (5'li ölçek veya açık uçlu sorular)
- 5'li ölçek sorularına SKS standardı seçeneklerin (Tamamen Katılıyorum → Kesinlikle Katılmıyorum, ağırlık 4-3-2-1-0) otomatik eklenmesi
- Anket yayınlama, arşivleme, arşivden geri alma
- Taslak durumundaki anketlerin silinmesi
- Hizmet türüne göre (Yatan / Ayaktan / Acil) filtreleme

### Davet ve Dağıtım
- Her davet için tek kullanımlık, kriptografik olarak güvenli token üretimi
- Token'dan otomatik QR kod (PNG) üretimi
- **İki kanallı** toplu davet gönderimi: SMS (İletiMerkezi API) ve e-posta (Gmail SMTP), aynı ekrandan kanal seçilerek
- Süresi dolan veya kullanılmış davetlerin otomatik geçersiz sayılması
- Kullanılmamış davetlerin silinebilmesi (kullanılmışlar korunur)

### Hasta Doldurma Deneyimi
- Kimlik veya iletişim bilgisi istemeyen, tamamen anonim doldurma ekranı
- Kart kart (adım adım) ilerleyen, mobil uyumlu form; her adım arası yumuşak geçiş animasyonu
- Canlı ilerleme çubuğu
- Gönderim öncesi "cevapları gözden geçir" ekranı
- Zorunlu sorular için hem tarayıcı hem sunucu tarafında doğrulama

### Raporlama
- SKS'nin resmi formülüyle (Σ ağırlık / (cevap sayısı × 4) × 100) otomatik karşılanma oranı hesabı
- Genel memnuniyet skoru ve toplam katılım özeti
- Soru bazında karşılanma oranı, düşük performans gösteren soruların kırmızı vurgulanması
- Hizmet türleri arası karşılaştırmalı rapor (Yatan / Ayaktan / Acil)
- Açık uçlu yorumların listelenmesi

### Aksiyon Takibi
- Karşılanma oranı %50'nin altında olan sorular için "Aksiyon Aç" özelliği
- Aksiyon durumu takibi (Açık / Devam Ediyor / Kapandı)

### Kullanıcı Yönetimi ve Güvenlik
- Kullanıcı adı / şifre ile giriş
- Rol tabanlı yetkilendirme (sıradan kullanıcı / yönetici)
- Yeni kullanıcı ekleme, pasifleştirme, aktifleştirme (sadece yönetici)
- Şifre değiştirme (kullanıcının kendisi) ve şifre sıfırlama (yönetici, unutma durumunda geçici şifre üretir)
- Şifrelerin `PasswordHasher` ile geri döndürülemez biçimde saklanması

### Denetim İzi
- Anket yayınlama/arşivleme, kullanıcı ekleme/pasifleştirme/aktifleştirme, aksiyon durumu değiştirme, davet silme, toplu davet gönderimi gibi kritik işlemlerin otomatik kaydı
- Kim, ne zaman, hangi işlemi yaptığı bilgisinin (kullanıcı adıyla) görüntülenmesi

### Kullanıcı Deneyimi Detayları
- Tarayıcının native `confirm()` kutuları yerine tema ile uyumlu özel onay modalı
- Kurumsal, mavi tonlu, tutarlı tasarım dili (bkz. [Tasarım / Tema](#tasarım--tema))

---

## Kullanılan Teknolojiler

| Katman | Teknoloji | Açıklama |
|---|---|---|
| Web çatısı | ASP.NET Core MVC (.NET 10) | Sunucu taraflı render, Controller/View ayrımı |
| Veritabanı | Oracle | Kurumsal standart; Docker (`gvenzl/oracle-free`) ile yerel geliştirme |
| ORM | Entity Framework Core (Oracle.EntityFrameworkCore) | Tip güvenli sorgular, LINQ |
| Kimlik doğrulama | ASP.NET Core Cookie Authentication | Oturum yönetimi |
| Yetkilendirme | Role-based Authorization (`[Authorize(Roles = "Yonetici")]`) | Yönetici / sıradan kullanıcı ayrımı |
| Şifre güvenliği | `Microsoft.Extensions.Identity.Core` (`PasswordHasher<T>`) | Endüstri standardı hash algoritması |
| QR üretimi | QRCoder | Sunucu taraflı PNG QR üretimi |
| E-posta | MailKit (Gmail SMTP) | Ücretsiz e-posta gönderimi |
| SMS | İletiMerkezi API (`HttpClient`) | Gerçek SMS gönderimi |
| Ön yüz | Bootstrap 5, Bootstrap Icons, özel CSS | Mavi tonlu kurumsal tema |
| API dokümantasyonu | Swashbuckle (Swagger) | Geliştirme ortamında `/swagger` |
| Mobil test | ngrok | Yerel geliştirme sunucusunu gerçek cihazla test etme |

---

## Mimari

Proje, **Onion Architecture** (Soğan Mimarisi) ile dört katmana ayrılmıştır. Bağımlılıklar her zaman **dıştan içe** doğru akar:

```
Probel.Survey.Web             →  Controller'lar, View'lar, routing, kimlik doğrulama
Probel.Survey.Infrastructure  →  EF Core, Oracle, QR/SMS/e-posta/token servisleri
Probel.Survey.Application     →  İş akışları (Service sınıfları), DTO'lar
Probel.Survey.Domain          →  Entity'ler, iş kuralları, arayüzler (dış bağımlılığı yoktur)
```

**Neden bu mimari:** İş kurallarının (örn. *"sorusu olmayan anket yayınlanamaz"*) veritabanı veya framework teknolojisinden bağımsız kalmasını sağlar. Oracle'dan farklı bir veritabanına geçilmesi gerekirse, yalnızca Infrastructure katmanı değişir; Domain ve Application katmanlarına dokunulmaz.

**Bildirim kanalı soyutlaması:** `IBildirimGonderici` arayüzünün arkasında, gelen kanal parametresine (`SMS` / `EPOSTA`) göre doğru göndericiye (`IletiMerkeziBildirimGonderici` / `GmailBildirimGonderici`) yönlendirme yapan bir `BildirimYonlendirici` sınıfı bulunur. Bu sayede Application katmanı, hangi kanalın arkada nasıl çalıştığını hiç bilmez.

---

## Veritabanı Şeması

Toplam **14 tablo**:

| Tablo | Amaç |
|---|---|
| `BIRIM` | Poliklinik/servis/klinik tanımı |
| `ROL`, `KULLANICI`, `KULLANICI_ROL` | Kullanıcı ve rol yönetimi altyapısı |
| `ANKET` | Anketin kalıcı kimliği |
| `ANKET_SURUM` | Yayınlanabilir/arşivlenebilir anket sürümü |
| `BOLUM`, `SORU`, `SORU_SECENEK` | Anket içeriği hiyerarşisi |
| `DAVET` | Token/QR kaydı |
| `YANIT_OTURUMU` | Anonim doldurma oturumu — **hiçbir kimlik alanı içermez** |
| `DEMOGRAFI` | İsteğe bağlı yaş/cinsiyet bilgisi |
| `YANIT` | Tek soruya verilen tek cevap |
| `AKSIYON` | Düşük puanlı soru için açılan takip kaydı |
| `DENETIM_IZI` | İşlem kaydı (kim, ne zaman, ne yaptı) |

Şema oluşturma script'i: `Scripts/01-oracle-sema.sql`

---

## Kurulum

### Gereksinimler

- .NET 10 SDK
- Docker Desktop (yerel Oracle için) veya erişilebilir bir Oracle veritabanı
- Visual Studio 2022 / 2026 (önerilir) veya VS Code + C# Dev Kit
- (İsteğe bağlı) Gmail hesabı — e-posta bildirimi için
- (İsteğe bağlı) İletiMerkezi hesabı — SMS bildirimi için

### Adım Adım Kurulum

**1. Depoyu klonlayın**
```bash
git clone <repo-adresi>
cd Probel.Survey
```

**2. Oracle veritabanını Docker ile başlatın**
```bash
docker run -d --name oracle-free -p 1521:1521 -e ORACLE_PASSWORD=<güçlü-bir-şifre> gvenzl/oracle-free
```
Konteynerin hazır olduğunu doğrulamak için:
```bash
docker logs -f oracle-free
```
`DATABASE IS READY TO USE!` mesajını gördükten sonra `Ctrl+C` ile çıkabilirsiniz.

**3. Bağlantı ve gizli bilgileri User Secrets'a ekleyin**

`Probel.Survey.Web` projesine sağ tıklayıp **Manage User Secrets** seçeneğini kullanın:
```json
{
  "ConnectionStrings": {
    "OracleDb": "User Id=system;Password=<şifreniz>;Data Source=//localhost:1521/FREEPDB1;"
  },
  "Email": {
    "Adres": "gönderici-adresiniz@gmail.com",
    "UygulamaSifresi": "Gmail Uygulama Şifreniz (16 haneli)"
  },
  "IletiMerkezi": {
    "ApiKey": "İletiMerkezi panelinizdeki API anahtarı",
    "ApiHash": "İletiMerkezi panelinizdeki hash",
    "Sender": "APITEST"
  },
  "PublicBaseUrl": ""
}
```
> Gmail Uygulama Şifresi: Google Hesabı → Güvenlik → 2 Adımlı Doğrulama açın → Uygulama Şifreleri sayfasından oluşturun.
> `PublicBaseUrl`, telefonla gerçek test yaparken ngrok adresinizle doldurulur (bkz. Adım 6).

**4. Veritabanı şemasını oluşturun**

`Scripts/01-oracle-sema.sql` dosyasını, Oracle'a bağlı bir SQL istemcisi (örn. VS Code Oracle eklentisi) üzerinden çalıştırın.

**5. Projeyi çalıştırın**

Visual Studio'da **Start**'a basın. Uygulama ilk kez çalıştığında henüz hiç kullanıcı olmadığı için:
```
https://localhost:<port>/Hesap/IlkKurulum
```
adresine giderek ilk yönetici hesabını oluşturun. Bu uç nokta, sistemde bir kullanıcı oluşturulduktan sonra kendini otomatik olarak devre dışı bırakır.

**6. (İsteğe bağlı) Telefonla test için ngrok**

```bash
ngrok http <port>
```
Elde ettiğiniz `https://xxxx.ngrok-free.app` adresini `appsettings.Development.json` içindeki `PublicBaseUrl` alanına yazın ve uygulamayı yeniden başlatın. Bu adres, üretilen QR kodların ve SMS/e-posta bağlantılarının içine otomatik olarak yazılır.

---

## Kullanım

### Kalite Birimi Çalışanı Olarak

1. Giriş yapın.
2. **Anketler** sayfasından yeni bir anket oluşturun, hizmet türünü seçin.
3. Anket detayına girip bölüm ve sorular ekleyin (ya da SKS soru setini toplu olarak yükleyin).
4. Anketi yayınlayın.
5. Davet/QR oluşturun veya **SMS ya da e-posta** ile toplu davet gönderin (kanal seçimi ekrandan yapılır).
6. Sonuçlar geldikçe **Rapor** ekranından karşılanma oranlarını izleyin.
7. Düşük puanlı bir konu için **Aksiyon Aç**ın ve takip edin.

### Yönetici Olarak (ek olarak)

- **Kullanıcılar** sayfasından yeni personel ekleyin, mevcut hesapları pasifleştirin/aktifleştirin.
- Şifresini unutan bir kullanıcı için geçici şifre üretip iletebilirsiniz.

### Hasta Olarak

- Size verilen QR kodu telefon kamerasıyla okutun ya da gelen SMS/e-postadaki bağlantıya tıklayın.
- Kimlik bilgisi girmeden, isteğe bağlı yaş/cinsiyet bilgisiyle anketi kart kart doldurun.
- Gönderdikten sonra aynı bağlantı tekrar kullanılamaz.

---

## Proje Yapısı

```
Probel.Survey.Domain/
  Entities/            Anket, AnketSurum, Bolum, Soru, SoruSecenek, Davet,
                        YanitOturumu, Demografi, Yanit, Aksiyon, DenetimIzi, Kullanici
  Repositories/         IAnketRepository, IKullaniciRepository
  Services/              ITokenUretici, IBildirimGonderici, IDenetimKaydedici

Probel.Survey.Application/
  Anketler/              AnketDtos, IAnketService, AnketService
  Kullanicilar/           IKullaniciService, KullaniciService

Probel.Survey.Infrastructure/
  Persistence/
    Configurations/        Her entity için EF Core eşleme sınıfları
    Repositories/           AnketRepository, KullaniciRepository
    SurveyDbContext.cs
  Services/                 TokenUretici, QrKodUretici, GmailBildirimGonderici,
                             IletiMerkeziBildirimGonderici, BildirimYonlendirici, DenetimKaydedici
  DependencyInjection.cs

Probel.Survey.Web/
  Controllers/               AnketController, KullaniciController, HesapController
  Views/
    Anket/                    Index, Detay, Rapor, Karsilastirma, Aksiyonlar, DenetimIzleri, Doldur, ...
    Kullanici/                 Index, Ekle
    Hesap/                      Giris, SifreDegistir
    Shared/                      _Layout (personel), _LayoutBos (hasta/giriş)
  wwwroot/css/site.css          Kurumsal mavi tema
```

---

## Tasarım / Tema

Arayüz, CSS değişkenleriyle (`wwwroot/css/site.css` içinde `:root` altında) yönetilen tutarlı bir **mavi kurumsal tema** kullanır:

- Koyu lacivert-mavi gradyanlı sabit sidebar, aktif sayfa açık mavi bir çizgiyle vurgulanır
- Kartlarda katmanlı, yumuşak gölgeler; istatistik kartlarının üstünde ince mavi bir şerit
- Butonlarda hover anında hafif yükselme efekti
- Doldurma ekranında adımlar arası yumuşak geçiş animasyonu
- Tarayıcının native `confirm()` penceresi yerine tema ile uyumlu özel onay modalı

Renk paleti tamamen `:root` içindeki CSS değişkenleri (`--pb-primary`, `--pb-sidebar` vb.) üzerinden değiştirilebilir; hiçbir `.cshtml` dosyasına dokunmaya gerek yoktur.

---

## Güvenlik ve Anonimlik

- **Tam anonimlik:** `YANIT_OTURUMU` tablosunda ad, telefon, e-posta gibi hiçbir kimlik alanı bulunmaz. Toplu davet gönderiminde kullanılan hedef adresler de veritabanına hiç yazılmaz.
- **Şifreler asla düz metin olarak saklanmaz** — `PasswordHasher<T>` ile geri döndürülemez hash üretilir.
- **Gizli bilgiler** (veritabanı şifresi, e-posta uygulama şifresi, SMS API anahtarı) koda veya `appsettings.json`'a değil, projeden ayrı tutulan **User Secrets** dosyasına yazılır; bu dosya Git'e dahil edilmez.
- **Token'lar** kriptografik olarak güvenli rastgele sayı üreteciyle (`RandomNumberGenerator`) oluşturulur, tahmin edilebilir değildir ve tek kullanımlıktır.
- **Rol tabanlı erişim:** Kullanıcı ekleme, pasifleştirme/aktifleştirme ve şifre sıfırlama işlemleri yalnızca yönetici rolüne sahip hesaplarla yapılabilir; bu kontrol hem arayüzde hem sunucu tarafında uygulanır.
- **Self-service "şifremi unuttum" bilinçli olarak eklenmemiştir** — sistemde kullanıcı e-postası tutulmadığı için bu, güvenlik açığı oluştururdu. Şifre sıfırlama yönetici aracılığıyla yapılır.

---

## Test

Proje, geliştirme sürecinde aşamalı olarak ve tamamlandıktan sonra uçtan uca olarak test edilmiştir:

- Üç hizmet türü (Yatan, Ayaktan, Acil) için gerçek SKS soru setleriyle tam senaryo testi
- Gerçek bir mobil cihazla (ngrok üzerinden) QR kod okutma testi
- Gerçek SMS ve e-posta gönderimi testleri
- Kenar durum testleri: süresi dolmuş/kullanılmış token, zorunlu soru doğrulaması, yetkisiz erişim denemeleri
- Rol tabanlı erişim kontrolü testleri

Detaylı test senaryoları ve sonuçları için proje test dokümantasyonuna bakınız.

---

## Bilinen Sınırlamalar

- Ücretsiz ngrok kullanıldığında, her yeni oturumda genel erişim adresi değişir; `PublicBaseUrl` her seferinde güncellenmelidir.
- E-posta gönderimi Gmail SMTP, SMS gönderimi İletiMerkezi üzerinden yapılmaktadır; büyük ölçekli/kurumsal gönderim için farklı bir sağlayıcıya geçilmesi gerekebilir (bu, yalnızca `Infrastructure` katmanında yeni bir sınıf yazmayı gerektirir).
- Self-service "şifremi unuttum" akışı bilinçli olarak eklenmemiştir; şifre sıfırlama yönetici aracılığıyla yapılır.

---

## Yol Haritası

Aşağıdaki maddeler, mevcut kapsamın dışında bırakılmış, gelecekte değerlendirilebilecek geliştirmelerdir:

- HBYS (Hastane Bilgi Yönetim Sistemi) entegrasyonu
- Koşullu soru akışı (bir cevaba göre sonraki sorunun değişmesi)
- Gerçek zamanlı (canlı güncellenen) dashboard

---

## Katkı ve İletişim

Bu proje, Probel bünyesinde bir staj çalışması olarak geliştirilmiştir.
