using Probel.Survey.Domain.Entities;


namespace Probel.Survey.Application.Kullanicilar;

public interface IKullaniciService
{
    Task<Kullanici?> GirisKontrolAsync(string kullaniciAdi, string sifre, CancellationToken ct = default);   // ← DEĞİŞTİ (artık bool değil, Kullanici? döndürüyor)
    Task KayitAsync(string kullaniciAdi, string sifre, string adSoyad, long? islemYapanId = null, CancellationToken ct = default);   // ← DEĞİŞTİ (islemYapanId eklendi)
    Task<IReadOnlyList<KullaniciListeDto>> GetAllAsync(CancellationToken ct = default);
    Task<bool> HicKullaniciVarMiAsync(CancellationToken ct = default);

}
public record KullaniciListeDto(long Id, string KullaniciAdi, string AdSoyad, bool AktifMi);