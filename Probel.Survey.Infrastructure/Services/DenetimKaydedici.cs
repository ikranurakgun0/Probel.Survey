using Probel.Survey.Domain.Entities;
using Probel.Survey.Domain.Services;
using Probel.Survey.Infrastructure.Persistence;

namespace Probel.Survey.Infrastructure.Services;

public class DenetimKaydedici : IDenetimKaydedici
{
    private readonly SurveyDbContext _db;
    public DenetimKaydedici(SurveyDbContext db) => _db = db;

    public async Task KaydetAsync(long? kullaniciId, string islem, string? hedefTablo = null, long? hedefId = null, CancellationToken ct = default)
    {
        var kayit = new DenetimIzi(kullaniciId, islem, hedefTablo, hedefId);
        await _db.DenetimIzleri.AddAsync(kayit, ct);
        await _db.SaveChangesAsync(ct); //Neden burada kendi SaveChangesAsync'ini çağırıyor, _repository.
                                        //SaveChangesAsync gibi ortak bir yerden değil: Denetim kaydı,
                                        //ait olduğu işlemden bağımsız,
                                        //kendi başına bir kayıt — asıl işlem
                                        //(örneğin yayınlama) başarıyla bittikten sonra çağrılacak,
                                        //o yüzden kendi transaction'ını kendi yönetmesi mantıklı.
    }
}