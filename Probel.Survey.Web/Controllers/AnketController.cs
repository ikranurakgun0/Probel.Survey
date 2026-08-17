using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Probel.Survey.Application.Anketler;
using Probel.Survey.Domain.Entities;
using Probel.Survey.Infrastructure.Services;
using System.Security.Claims;



namespace Probel.Survey.Web.Controllers;

[Authorize]   // ← YENİ — artık sınıftaki HER action, varsayılan olarak giriş ister
public class AnketController : Controller
{
    private readonly IAnketService _anketService;

    public AnketController(IAnketService anketService)
        => _anketService = anketService;
    private long? GetMevcutKullaniciId()
    {
        var deger = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(deger, out var id) ? id : null; //Bu metot ne işe yarıyor: Az önce çerezle taşıdığımız ID'yi, Controller
                                                             //içinde okuyup C#'ta kullanılabilir bir long?'a çeviriyor —
                                                             //her action'da tekrar tekrar aynı kodu yazmamak için bir yardımcı.
    }

    public async Task<IActionResult> Index(string? hizmetTuru, CancellationToken ct)
    {
        var anketler = await _anketService.GetAllAsync(ct);

        if (!string.IsNullOrWhiteSpace(hizmetTuru))
            anketler = anketler.Where(a => a.HizmetTuru == hizmetTuru).ToList();

        ViewBag.SeciliHizmetTuru = hizmetTuru;
        return View(anketler);
    }
    public IActionResult Olustur() => View(); // GET — formu göster

    [HttpPost]
    public async Task<IActionResult> Olustur(string ad, string? hizmetTuru, CancellationToken ct) // POST — formu işle
    {
        if (string.IsNullOrWhiteSpace(ad))
        {
            ModelState.AddModelError("", "Anket adı zorunludur.");
            return View();
        }

        await _anketService.CreateAsync(ad, hizmetTuru, ct);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> Yayinla(long id, CancellationToken ct)
    {
        try
        {
            await _anketService.YayinlaAsync(id, GetMevcutKullaniciId(), ct);
            TempData["Mesaj"] = "Anket yayınlandı.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));  //TempData neden var, ViewBag değil: Hatırlarsan
                                                 //Yayinla action'ı işini bitirince RedirectToAction(nameof(Index)) ile
                                                 //başka bir action'a yönlendiriyor — yani yeni bir HTTP isteği başlıyor.
                                                 //ViewBag/ViewData sadece tek bir istek boyunca yaşar,
                                                 //yönlendirme sonrası kaybolur.
                                                 //TempData ise "bir sonraki isteğe kadar"
                                                 //yaşayan özel bir depo — tam olarak bu senaryo için var.
    }
    public async Task<IActionResult> Detay(long id, CancellationToken ct)
    {
        var detay = await _anketService.GetDetayAsync(id, ct);
        var davetler = await _anketService.GetDavetlerAsync(id, ct);
        ViewBag.Davetler = davetler;
        return View(detay);
    }

    [HttpPost]
    public async Task<IActionResult> BolumEkle(long anketSurumId, string ad, CancellationToken ct)
    {
        await _anketService.BolumEkleAsync(anketSurumId, ad, ct); //← burada Application katmanına geçiliyor
        return RedirectToAction(nameof(Detay), new { id = anketSurumId });
    }

    [HttpPost]
    public async Task<IActionResult> SoruEkle(long anketSurumId, long bolumId, string metin, SoruTipi tip, bool zorunluMu, CancellationToken ct)
    {
        await _anketService.SoruEkleAsync(anketSurumId, bolumId, metin, tip, zorunluMu, ct);
        return RedirectToAction(nameof(Detay), new { id = anketSurumId });
    }
    [HttpPost]
    public async Task<IActionResult> DavetOlustur(long anketSurumId, CancellationToken ct)
    {
        try
        {
            await _anketService.DavetOlusturAsync(anketSurumId, ct);
            TempData["Mesaj"] = "Davet oluşturuldu.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Detay), new { id = anketSurumId });
    }   //İşlem bitince (başarılı ya da başarısız fark etmez),
        //kullanıcıyı aynı anketin Detay sayfasına geri gönderiyor.
        //new { id = anketSurumId },
        //Detay action'ının beklediği id parametresini otomatik dolduruyor —
        //yani /Anket/Detay/5 gibi doğru adrese gidiyor.

    [AllowAnonymous]
    public IActionResult QrKod(string token, [FromServices] IQrKodUretici qrUretici, [FromServices] IConfiguration config)
    {
        var publicBase = config["PublicBaseUrl"];

        string url = !string.IsNullOrWhiteSpace(publicBase)
            ? $"{publicBase.TrimEnd('/')}/Anket/Doldur?token={token}"
            : Url.Action("Doldur", "Anket", new { token }, Request.Scheme)!;

        var png = qrUretici.Uret(url);
        return File(png, "image/png");
    }
    [AllowAnonymous]
    public async Task<IActionResult> Doldur(string token, CancellationToken ct)
    {
        try
        {
            var dto = await _anketService.DoldurmaBaslatAsync(token, ct);
            return View(dto);
        }
        catch (Exception ex) when (ex is KeyNotFoundException or InvalidOperationException)
        {
            return View("DoldurmaHata", ex.Message);
        }
    }

    [HttpPost]
    [AllowAnonymous]
    [ActionName("Doldur")]
    public async Task<IActionResult> DoldurKaydet(string token, CancellationToken ct)
    {
        var form = await Request.ReadFormAsync(ct);
        var cevaplar = new List<CevapGirisi>();

        foreach (var key in form.Keys)
        {
            if (key.StartsWith("secenek_") && long.TryParse(key.AsSpan(8), out var soruId1))
            {
                if (long.TryParse(form[key], out var secenekId))
                    cevaplar.Add(new CevapGirisi(soruId1, secenekId, null));
            }
            else if (key.StartsWith("metin_") && long.TryParse(key.AsSpan(6), out var soruId2))
            {
                var metin = form[key].ToString();
                if (!string.IsNullOrWhiteSpace(metin))
                    cevaplar.Add(new CevapGirisi(soruId2, null, metin));
            }
        }

        var cinsiyet = form["cinsiyet"].ToString();
        var yasAraligi = form["yasAraligi"].ToString();

        try
        {
            await _anketService.GonderAsync(
                token, cevaplar,
                string.IsNullOrWhiteSpace(cinsiyet) ? null : cinsiyet,
                string.IsNullOrWhiteSpace(yasAraligi) ? null : yasAraligi,
                ct);

            return RedirectToAction(nameof(Tesekkur));
        }
        catch (Exception ex)
        {
            return View("DoldurmaHata", ex.Message);
        }
    }
    public async Task<IActionResult> Rapor(long id, CancellationToken ct)
    {
        var rapor = await _anketService.GetRaporAsync(id, ct);
        return View(rapor);
    }
    [AllowAnonymous]
    public IActionResult Tesekkur() => View();

    [HttpPost]
    public async Task<IActionResult> AksiyonAc(long soruId, string aciklama, DateTime? hedefTarih, long anketSurumId, CancellationToken ct)
    {
        await _anketService.AksiyonAcAsync(soruId, aciklama, hedefTarih, ct);
        TempData["Mesaj"] = "Aksiyon açıldı.";
        return RedirectToAction(nameof(Rapor), new { id = anketSurumId });
    }
   
    public async Task<IActionResult> Aksiyonlar(CancellationToken ct)
    {
        var aksiyonlar = await _anketService.GetAksiyonlarAsync(ct);
        return View(aksiyonlar);
    }

    [HttpPost]
    
    public async Task<IActionResult> AksiyonDurumGuncelle(long aksiyonId, string yeniDurum, CancellationToken ct)
    {
        await _anketService.AksiyonDurumGuncelleAsync(aksiyonId, yeniDurum, GetMevcutKullaniciId(), ct);
        TempData["Mesaj"] = "Durum güncellendi.";
        return RedirectToAction(nameof(Aksiyonlar));
    }

    public async Task<IActionResult> DenetimIzleri(CancellationToken ct)
    {
        var kayitlar = await _anketService.GetDenetimIzleriAsync(ct);
        return View(kayitlar);
    }
  
    public async Task<IActionResult> Karsilastirma(CancellationToken ct)
    {
        var rapor = await _anketService.GetKarsilastirmaRaporuAsync(ct);
        return View(rapor);
    }
    
    [HttpPost]
    public async Task<IActionResult> Sil(long id, CancellationToken ct)
    {
        try
        {
            await _anketService.AnketSilAsync(id, ct);
            TempData["Mesaj"] = "Anket silindi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    public async Task<IActionResult> Arsivle(long id, CancellationToken ct)
    {
        try
        {
            await _anketService.ArsivleAsync(id, GetMevcutKullaniciId(), ct);
            TempData["Mesaj"] = "Anket arşivlendi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DavetSil(long davetId, long anketSurumId, CancellationToken ct)
    {
        try
        {
            await _anketService.DavetSilAsync(davetId, GetMevcutKullaniciId(), ct);
            TempData["Mesaj"] = "Davet silindi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Detay), new { id = anketSurumId });
    }

    [HttpPost]
    public async Task<IActionResult> TopluDavetOlustur(long anketSurumId, string hedefListesi, CancellationToken ct)
    {
        var hedefler = hedefListesi
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();

        try
        {  //GetMevcutKullaniciId() — hatırlarsan bu, Yayınla/Arşivle
           //action'larında da kullandığımız, giriş yapmış
           //kullanıcının ID'sini çerezden okuyan yardımcı metot,
           //aynı Controller'ın içinde zaten tanımlıydı.
            var sonuclar = await _anketService.TopluDavetOlusturAsync(anketSurumId, hedefler, GetMevcutKullaniciId(), ct);
            TempData["TopluSonuc"] = System.Text.Json.JsonSerializer.Serialize(sonuclar);
            TempData["Mesaj"] = $"{sonuclar.Count(s => s.BasariliMi)}/{sonuclar.Count} e-posta gönderildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(Detay), new { id = anketSurumId });
    }
}