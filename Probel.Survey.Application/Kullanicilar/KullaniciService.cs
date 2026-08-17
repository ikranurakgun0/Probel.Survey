using Microsoft.AspNetCore.Identity;
using Probel.Survey.Domain.Entities;
using Probel.Survey.Domain.Repositories;
using Probel.Survey.Domain.Services;

namespace Probel.Survey.Application.Kullanicilar;

public class KullaniciService : IKullaniciService
{
    private readonly IKullaniciRepository _repository;
    private readonly IDenetimKaydedici _denetim;
    private readonly PasswordHasher<Kullanici> _hasher = new();

    public KullaniciService(IKullaniciRepository repository, IDenetimKaydedici denetim)
    {
        _repository = repository;
        _denetim = denetim;
    }

    public async Task<Kullanici?> GirisKontrolAsync(string kullaniciAdi, string sifre, CancellationToken ct = default)
    {
        var kullanici = await _repository.GetByKullaniciAdiAsync(kullaniciAdi, ct);
        if (kullanici == null || !kullanici.AktifMi)
            return null;

        var sonuc = _hasher.VerifyHashedPassword(kullanici, kullanici.SifreHash, sifre);
        return sonuc == PasswordVerificationResult.Success ? kullanici : null;
    }

    public async Task KayitAsync(string kullaniciAdi, string sifre, string adSoyad, bool yoneticiMi = false, long? islemYapanId = null, CancellationToken ct = default)
    {
        var mevcut = await _repository.GetByKullaniciAdiAsync(kullaniciAdi, ct);
        if (mevcut != null)
            throw new InvalidOperationException("Bu kullanıcı adı zaten kayıtlı.");

        var gecici = new Kullanici(kullaniciAdi, "", adSoyad);
        var hash = _hasher.HashPassword(gecici, sifre);
        var kullanici = new Kullanici(kullaniciAdi, hash, adSoyad, yoneticiMi);

        await _repository.AddAsync(kullanici, ct);
        await _repository.SaveChangesAsync(ct);

        await _denetim.KaydetAsync(islemYapanId, "KULLANICI_EKLE", "KULLANICI", kullanici.Id, ct);
    }

    public async Task<IReadOnlyList<KullaniciListeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var kullanicilar = await _repository.GetAllAsync(ct);
        return kullanicilar
            .Select(k => new KullaniciListeDto(k.Id, k.KullaniciAdi, k.AdSoyad, k.AktifMi, k.YoneticiMi))
            .ToList();
    }

    public Task<bool> HicKullaniciVarMiAsync(CancellationToken ct = default)
        => _repository.HicKullaniciVarMiAsync(ct);

    public async Task PasiflestirAsync(long kullaniciId, long? islemYapanId, CancellationToken ct = default)
    {
        var kullanici = await _repository.GetByIdAsync(kullaniciId, ct)
                        ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        kullanici.Pasiflestir();
        await _repository.SaveChangesAsync(ct);

        await _denetim.KaydetAsync(islemYapanId, "KULLANICI_PASIFLESTIR", "KULLANICI", kullaniciId, ct);
    }
    public async Task AktiflestirAsync(long kullaniciId, long? islemYapanId, CancellationToken ct = default)
    {
        var kullanici = await _repository.GetByIdAsync(kullaniciId, ct)
                        ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        kullanici.Aktiflestir();
        await _repository.SaveChangesAsync(ct);

        await _denetim.KaydetAsync(islemYapanId, "KULLANICI_AKTIFLESTIR", "KULLANICI", kullaniciId, ct);
    }
    public async Task SifreDegistirAsync(long kullaniciId, string mevcutSifre, string yeniSifre, CancellationToken ct = default)
    {
        var kullanici = await _repository.GetByIdAsync(kullaniciId, ct)
                        ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var sonuc = _hasher.VerifyHashedPassword(kullanici, kullanici.SifreHash, mevcutSifre);
        if (sonuc != PasswordVerificationResult.Success)
            throw new InvalidOperationException("Mevcut şifreniz hatalı.");

        var yeniHash = _hasher.HashPassword(kullanici, yeniSifre);
        kullanici.SifreGuncelle(yeniHash);
        await _repository.SaveChangesAsync(ct);

        await _denetim.KaydetAsync(kullaniciId, "SIFRE_DEGISTIR", "KULLANICI", kullaniciId, ct);
    }
    public async Task<string> SifreSifirlaAsync(long kullaniciId, long? islemYapanId, CancellationToken ct = default)
    {
        var kullanici = await _repository.GetByIdAsync(kullaniciId, ct)
                        ?? throw new KeyNotFoundException("Kullanıcı bulunamadı.");

        var yeniSifre = GeciciSifreUret();
        var yeniHash = _hasher.HashPassword(kullanici, yeniSifre);
        kullanici.SifreGuncelle(yeniHash);
        await _repository.SaveChangesAsync(ct);

        await _denetim.KaydetAsync(islemYapanId, "SIFRE_SIFIRLA", "KULLANICI", kullaniciId, ct);

        return yeniSifre;
    }

    private static string GeciciSifreUret()
    {
        const string karakterler = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz23456789";
        var bytes = System.Security.Cryptography.RandomNumberGenerator.GetBytes(10);
        var sb = new System.Text.StringBuilder();
        foreach (var b in bytes)
            sb.Append(karakterler[b % karakterler.Length]);
        return sb.ToString();
    }
}