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
            new(ClaimTypes.Name, kullaniciAdi),
            new(ClaimTypes.NameIdentifier, kullanici.Id.ToString()) //ClaimTypes.NameIdentifier ne işe yarıyor:
                                                                    //Bu, giriş çerezine "bu kullanıcının ID'si şu"
                                                                    //bilgisini gömüyor. Artık her sayfada,
                                                                    //User.FindFirst(ClaimTypes.NameIdentifier) diyerek,
                                                                    //o an giriş yapmış kullanıcının ID'sine ulaşabiliyoruz
        };
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

    // GEÇİCİ: ilk kullanıcıyı oluşturmak için. Kullandıktan sonra SİL.
    [AllowAnonymous]
    public async Task<IActionResult> IlkKurulum(CancellationToken ct)
    {
        var hicKullaniciVarMi = await _kullaniciService.HicKullaniciVarMiAsync(ct);
        if (hicKullaniciVarMi)
            return Forbid();   // sistemde zaten kullanıcı varsa, bu adres artık çalışmaz

        await _kullaniciService.KayitAsync("admin", "Sifre123!", "Sistem Yöneticisi", null, ct);
        return Content("İlk yönetici hesabı oluşturuldu: admin / Sifre123!");
    }

    [AllowAnonymous]
    public IActionResult ErisimYok() => View();
}