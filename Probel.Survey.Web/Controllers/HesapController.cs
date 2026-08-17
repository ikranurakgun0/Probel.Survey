using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Probel.Survey.Application.Kullanicilar;
using System.Security.Claims;

namespace Probel.Survey.Web.Controllers;

public class HesapController : Controller
{
    private readonly IKullaniciService _kullaniciService;
    public HesapController(IKullaniciService kullaniciService) => _kullaniciService = kullaniciService;

    [AllowAnonymous]
    public IActionResult Giris() => View();

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> Giris(string kullaniciAdi, string sifre, CancellationToken ct)
    {
        var kullanici = await _kullaniciService.GirisKontrolAsync(kullaniciAdi, sifre, ct);
        if (kullanici == null)
        {
            ModelState.AddModelError("", "Kullanıcı adı veya şifre hatalı.");
            return View();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, kullanici.KullaniciAdi),
            new(ClaimTypes.NameIdentifier, kullanici.Id.ToString())
        };
        if (kullanici.YoneticiMi)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Yonetici"));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(new ClaimsPrincipal(identity));

        return RedirectToAction("Index", "Anket");
    }

    [Authorize]
    public async Task<IActionResult> Cikis()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction(nameof(Giris));
    }

    [AllowAnonymous]
    public async Task<IActionResult> IlkKurulum(CancellationToken ct)
    {
        var hicKullaniciVarMi = await _kullaniciService.HicKullaniciVarMiAsync(ct);
        if (hicKullaniciVarMi)
            return Forbid();

        await _kullaniciService.KayitAsync("admin", "Sifre123!", "Sistem Yöneticisi", yoneticiMi: true, islemYapanId: null, ct: ct);
        return Content("İlk yönetici hesabı oluşturuldu: admin / Sifre123!");
    }
    [Authorize]
    public IActionResult SifreDegistir() => View();

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> SifreDegistir(string mevcutSifre, string yeniSifre, string yeniSifreTekrar, CancellationToken ct)
    {
        if (yeniSifre != yeniSifreTekrar)
        {
            TempData["Hata"] = "Yeni şifreler eşleşmiyor.";
            return RedirectToAction(nameof(SifreDegistir));
        }

        var kullaniciIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(kullaniciIdStr, out var kullaniciId))
            return Forbid();

        try
        {
            await _kullaniciService.SifreDegistirAsync(kullaniciId, mevcutSifre, yeniSifre, ct);
            TempData["Mesaj"] = "Şifreniz güncellendi.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or KeyNotFoundException)
        {
            TempData["Hata"] = ex.Message;
        }

        return RedirectToAction(nameof(SifreDegistir));
    }
}