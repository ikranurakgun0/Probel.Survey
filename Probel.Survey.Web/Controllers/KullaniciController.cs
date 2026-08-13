using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Probel.Survey.Application.Kullanicilar;

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

    public IActionResult Ekle() => View();

    [HttpPost]
    public async Task<IActionResult> Ekle(string kullaniciAdi, string sifre, string adSoyad, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(kullaniciAdi) || string.IsNullOrWhiteSpace(sifre))
        {
            ModelState.AddModelError("", "Kullanıcı adı ve şifre zorunludur.");
            return View();
        }

        var islemYapanId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        long? islemYapanIdLong = long.TryParse(islemYapanId, out var id) ? id : null;

        try
        {
            await _kullaniciService.KayitAsync(kullaniciAdi, sifre, adSoyad, islemYapanIdLong, ct);
            TempData["Mesaj"] = "Kullanıcı eklendi.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            TempData["Hata"] = ex.Message;
            return View();
        }
    }
}
