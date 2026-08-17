using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Domain.Repositories;

public interface IAnketRepository //Anket repositorynin ne yapacağını söyler nasıl yapacağını söylemez.Sadece "veriye erişim" işi için kullanılır.İnterfacein özel kullanım şeklidir.
{
    Task<AnketSurum?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<IReadOnlyList<AnketSurumOzet>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(AnketSurum anketSurum, CancellationToken ct = default);
    Task AddAnketAsync(Anket anket, CancellationToken ct = default);
    Task AddDavetAsync(Davet davet, CancellationToken ct = default);
    Task<IReadOnlyList<Davet>> GetDavetlerAsync(long anketSurumId, CancellationToken ct = default);
    Task<Davet?> GetDavetByTokenAsync(string token, CancellationToken ct = default);        
    Task AddYanitOturumuAsync(YanitOturumu oturum, CancellationToken ct = default);
    Task<IReadOnlyList<YanitKaydi>> GetYanitlarAsync(long anketSurumId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    Task AddAksiyonAsync(Aksiyon aksiyon, CancellationToken ct = default);
    Task<IReadOnlyList<Aksiyon>> GetAksiyonlarAsync(CancellationToken ct = default);
    Task<Aksiyon?> GetAksiyonByIdAsync(long id, CancellationToken ct = default);
    Task<string?> GetSoruMetniAsync(long soruId, CancellationToken ct = default);
    Task<IReadOnlyList<DenetimIziKaydi>> GetDenetimIzleriAsync(CancellationToken ct = default);
    //Aksiyon metotlarını da aynı arayüze eklediğimiz için,
    //tutarlılık adına aynı yerde tutuyoruz proje genişlediğinde ayrı bir repositoryye taşımak daha işlevsel ve mantıklı olabilir.
    Task SilAsync(long anketSurumId, CancellationToken ct = default);
    Task<Davet?> GetDavetByIdAsync(long davetId, CancellationToken ct = default);
    Task DavetSilAsync(long davetId, CancellationToken ct = default);
    Task<AnketOzetBilgi?> GetAnketBilgisiAsync(long anketId, CancellationToken ct = default);
}
public record YanitKaydi(long YanitOturumuId, long SoruId, string SoruMetni, string SoruTipi, int? Agirlik, string? MetinDeger);
//YanitKaydi neden burada, Application'da değil: Bu,
//IAnketRepository'nin dönüş tipi. Arayüz Domain'de olduğu için,
//onun kullandığı tip de aynı projede olmalı —
//aksi halde Domain, Application'a bağımlı olurdu, tersi olması gerekirken.
//Bu bir Domain entity'si değil, davranışı yok, sadece rapor hesaplaması için ara bir taşıyıcı.

public record AnketSurumOzet(long Id, int SurumNo, string Durum, string AnketAdi, string? HizmetTuru);
public record AnketOzetBilgi(string Ad, string? HizmetTuru);
public record DenetimIziKaydi(long Id, long? KullaniciId, string? KullaniciAdi, string Islem, string? HedefTablo, long? HedefId, DateTime Zaman);