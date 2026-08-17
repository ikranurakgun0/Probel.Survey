using Probel.Survey.Domain.Entities;

namespace Probel.Survey.Domain.Repositories;

public interface IKullaniciRepository
{

    Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi, CancellationToken ct = default);
    Task AddAsync(Kullanici kullanici, CancellationToken ct = default);
    Task<IReadOnlyList<Kullanici>> GetAllAsync(CancellationToken ct = default);
    Task<Kullanici?> GetByIdAsync(long id, CancellationToken ct = default);
    Task<bool> HicKullaniciVarMiAsync(CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
    //Neden ayrı bir Repository, IAnketRepository'ye eklemedik: Kullanici,
    //anketlerle doğrudan ilgili değil
    //— ayrı bir sorumluluk alanı (kimlik doğrulama),
    //bu yüzden kendi Repository'sini hak ediyor.
    //Aynı IAnketRepository'ye eklemek, o arayüzü gereksiz şişirirdi.
}