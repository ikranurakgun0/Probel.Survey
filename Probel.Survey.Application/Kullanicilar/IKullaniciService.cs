namespace Probel.Survey.Application.Kullanicilar;

public interface IKullaniciService
{
    Task<Probel.Survey.Domain.Entities.Kullanici?> GirisKontrolAsync(string kullaniciAdi, string sifre, CancellationToken ct = default);
    Task KayitAsync(string kullaniciAdi, string sifre, string adSoyad, bool yoneticiMi = false, long? islemYapanId = null, CancellationToken ct = default);
    Task<IReadOnlyList<KullaniciListeDto>> GetAllAsync(CancellationToken ct = default);
    Task<bool> HicKullaniciVarMiAsync(CancellationToken ct = default);
    Task PasiflestirAsync(long kullaniciId, long? islemYapanId, CancellationToken ct = default);
    Task AktiflestirAsync(long kullaniciId, long? islemYapanId, CancellationToken ct = default);
}

public record KullaniciListeDto(long Id, string KullaniciAdi, string AdSoyad, bool AktifMi, bool YoneticiMi);