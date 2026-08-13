namespace Probel.Survey.Application.Anketler;

public interface IAnketService
{
    Task<IReadOnlyList<AnketListeDto>> GetAllAsync(CancellationToken ct = default);
    Task<long> CreateAsync(string ad, string? hizmetTuru, CancellationToken ct = default);
    Task YayinlaAsync(long id, long? kullaniciId, CancellationToken ct = default);
    Task<AnketDetayDto> GetDetayAsync(long id, CancellationToken ct = default);
    Task BolumEkleAsync(long anketSurumId, string ad, CancellationToken ct = default);
    Task SoruEkleAsync(long anketSurumId, long bolumId, string metin, Probel.Survey.Domain.Entities.SoruTipi tip, bool zorunluMu, CancellationToken ct = default);
    Task<string> DavetOlusturAsync(long anketSurumId, CancellationToken ct = default);
    Task<IReadOnlyList<DavetDto>> GetDavetlerAsync(long anketSurumId, CancellationToken ct = default);
    Task<AnketDoldurmaDto> DoldurmaBaslatAsync(string token, CancellationToken ct = default);
    Task GonderAsync(string token, List<CevapGirisi> cevaplar, string? cinsiyet, string? yasAraligi, CancellationToken ct = default);
    Task<AnketRaporDto> GetRaporAsync(long anketSurumId, CancellationToken ct = default);
    Task AksiyonAcAsync(long soruId, string aciklama, DateTime? hedefTarih, CancellationToken ct = default);
    Task<IReadOnlyList<AksiyonListeDto>> GetAksiyonlarAsync(CancellationToken ct = default);
    Task AksiyonDurumGuncelleAsync(long aksiyonId, string yeniDurum, long? kullaniciId, CancellationToken ct = default);
    Task<IReadOnlyList<DenetimIziDto>> GetDenetimIzleriAsync(CancellationToken ct = default);
}