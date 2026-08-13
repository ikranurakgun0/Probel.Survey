using Microsoft.EntityFrameworkCore;
using Probel.Survey.Domain.Entities;
using Probel.Survey.Domain.Repositories;

namespace Probel.Survey.Infrastructure.Persistence.Repositories;

public class KullaniciRepository : IKullaniciRepository
{
    private readonly SurveyDbContext _db;
    public KullaniciRepository(SurveyDbContext db) => _db = db;

    public Task<Kullanici?> GetByKullaniciAdiAsync(string kullaniciAdi, CancellationToken ct = default)
        => _db.Kullanicilar.FirstOrDefaultAsync(k => k.KullaniciAdi == kullaniciAdi, ct);


    public async Task AddAsync(Kullanici kullanici, CancellationToken ct = default)
        => await _db.Kullanicilar.AddAsync(kullanici, ct);

    public async Task<IReadOnlyList<Kullanici>> GetAllAsync(CancellationToken ct = default)
        => await _db.Kullanicilar.AsNoTracking().ToListAsync(ct);

    public async Task<bool> HicKullaniciVarMiAsync(CancellationToken ct = default)
        => await _db.Kullanicilar.AnyAsync(ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}