using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Probel.Survey.Application.Kullanicilar;
using System.Security.Claims;

namespace Probel.Survey.Web.Controllers;

[Authorize]
public class KullaniciController : Controller
{
    private readonly IKullaniciService _kullaniciService;
    public KullaniciController(IKullaniciService kullaniciService) => _kullaniciService = kullaniciService;

    
    public async Task<IActionResult> Index(CancellationToken ct)
    {
        var kullanicilar = await _kullaniciService.GetAllAsync(ct);
        return View(kullanicilar);
    }

    [Authorize(Roles = "Yonetici")]
    public IActionResult Ekle() => View();

    [HttpPost]
    [Authorize(Roles = "Yonetici")]
    public async Task<IActionResult> Ekle(string kullaniciAdi, string sifre, string adSoyad, bool yoneticiMi, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(sifre))
        {
            ModelState.AddModelError("", "Kullanıcı adı ve şifre zorunludur.");
            return View();
        }

        var islemYapanId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long? islemYapanIdLong = long.TryParse(islemYapanId, out var id) ? id : null;

        try
        {
            await _kullaniciService.KayitAsync(kullaniciAdi, sifre, adSoyad, yoneticiMi, islemYapanIdLong, ct);
            TempData["Mesaj"] = "Kullanıcı eklendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
            return View();
        }
    }

    [HttpPost]
    [Authorize(Roles = "Yonetici")]
    public async Task<IActionResult> Pasiflestir(long id, CancellationToken ct)
    {
        var islemYapanId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long? islemYapanIdLong = long.TryParse(islemYapanId, out var pid) ? pid : null;

        try
        {
            await _kullaniciService.PasiflestirAsync(id, islemYapanIdLong, ct);
            TempData["Mesaj"] = "Kullanıcı pasifleştirildi.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
        }
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [Authorize(Roles = "Yonetici")]
    public async Task<IActionResult> Aktiflestir(long id, CancellationToken ct)
    {
        var islemYapanId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long? islemYapanIdLong = long.TryParse(islemYapanId, out var pid) ? pid : null;

        await _kullaniciService.AktiflestirAsync(id, islemYapanIdLong, ct);
        TempData["Mesaj"] = "Kullanıcı aktifleştirildi.";
        return RedirectToAction(nameof(Index));
    }
    [HttpPost]
    [Authorize(Roles = "Yonetici")]
    public async Task<IActionResult> SifreSifirla(long id, CancellationToken ct)
    {
        var islemYapanId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        long? islemYapanIdLong = long.TryParse(islemYapanId, out var pid) ? pid : null;

        var yeniSifre = await _kullaniciService.SifreSifirlaAsync(id, islemYapanIdLong, ct);
        TempData["Mesaj"] = $"Yeni geçici şifre: {yeniSifre} — bu şifreyi kullanıcıya güvenli bir kanaldan iletin, bir daha ekranda görünmeyecek.";
        return RedirectToAction(nameof(Index));
    }
}