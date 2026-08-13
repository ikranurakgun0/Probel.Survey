using Microsoft.AspNetCore.Identity;
using Probel.Survey.Domain.Entities;
using Probel.Survey.Domain.Repositories;
using Probel.Survey.Domain.Services;

namespace Probel.Survey.Application.Kullanicilar;

public class KullaniciService : IKullaniciService
{
    private readonly IKullaniciRepository _repository;
    private readonly PasswordHasher<Kullanici> _hasher = new();
    private readonly IDenetimKaydedici _denetim;

    public KullaniciService(IKullaniciRepository repository, IDenetimKaydedici denetim)   // ← DEĞİŞTİ
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

    public async Task KayitAsync(string kullaniciAdi, string sifre, string adSoyad, long? islemYapanId = null, CancellationToken ct = default)
    {
        var mevcut = await _repository.GetByKullaniciAdiAsync(kullaniciAdi, ct);
        if (mevcut != null)
            throw new InvalidOperationException("Bu kullanıcı adı zaten kayıtlı.");

        var gecici = new Kullanici(kullaniciAdi, "", adSoyad);
        var hash = _hasher.HashPassword(gecici, sifre);
        var kullanici = new Kullanici(kullaniciAdi, hash, adSoyad);

        await _repository.AddAsync(kullanici, ct);
        await _repository.SaveChangesAsync(ct);
        await _denetim.KaydetAsync(islemYapanId, "KULLANICI_EKLE", "KULLANICI", kullanici.Id, ct);
    }
    public async Task<IReadOnlyList<KullaniciListeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var kullanicilar = await _repository.GetAllAsync(ct);
        return kullanicilar
            .Select(k => new KullaniciListeDto(k.Id, k.KullaniciAdi, k.AdSoyad, k.AktifMi))
            .ToList();
    }

    public Task<bool> HicKullaniciVarMiAsync(CancellationToken ct = default)
        => _repository.HicKullaniciVarMiAsync(ct);
}