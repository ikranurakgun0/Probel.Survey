using Microsoft.EntityFrameworkCore;
using Probel.Survey.Domain.Entities;
using Probel.Survey.Domain.Repositories;

namespace Probel.Survey.Infrastructure.Persistence.Repositories;

public class AnketRepository : IAnketRepository //IAnketRepository'nin sözleşmesini baz alarak gerçek işi yapan kod. biz Oracle'a erişimi sadece AnketSurum üzerinden kuruyoruz
                                                //(repository sadece AnketSurum odaklı),
                                                //bu yüzden AnketService'de önce ana nesneye, oradan alt nesneye iniyoruz.
{
    private readonly SurveyDbContext _db;
    public AnketRepository(SurveyDbContext db) => _db = db;

    public Task<AnketSurum?> GetByIdAsync(long id, CancellationToken ct = default)
       => _db.AnketSurumleri
             .Include(a => a.Bolumler)
                 .ThenInclude(b => b.Sorular)          // Artık sadece bölümleri değil, bölümlerin sorularını da çekebiliyoruz. 
                    .ThenInclude(s => s.Secenekler)
             .FirstOrDefaultAsync(a => a.Id == id, ct);


    public async Task<IReadOnlyList<AnketSurumOzet>> GetAllAsync(CancellationToken ct = default)//GetAllAsync metodu IAnketRepository sözleşmesini uygular.
    {
        var query =
            from asv in _db.AnketSurumleri.AsNoTracking()//Güncellemeyip sadece okumak için AsNoTracking() kullanılır. Bu performansı artırır.

            join a in _db.Anketler.AsNoTracking() on asv.AnketId equals a.Id
            orderby asv.Id descending //orderby asv.Id descending ile en son oluşturulan anket
                                      //listenin en üstünde görünüyor — küçük ama pratik bir ek iyileştirme.
            select new AnketSurumOzet(asv.Id, asv.SurumNo, asv.Durum.ToString(), a.Ad, a.HizmetTuru);

        return await query.ToListAsync(ct);
    }
    public async Task AddAsync(AnketSurum anketSurum, CancellationToken ct = default)
        => await _db.AnketSurumleri.AddAsync(anketSurum, ct);
    public async Task AddAnketAsync(Anket anket, CancellationToken ct = default)
        => await _db.Anketler.AddAsync(anket, ct);
    public async Task AddDavetAsync(Davet davet, CancellationToken ct = default)
    => await _db.Davetler.AddAsync(davet, ct);

    public async Task<IReadOnlyList<Davet>> GetDavetlerAsync(long anketSurumId, CancellationToken ct = default)
        => await _db.Davetler
                    .Where(d => d.AnketSurumId == anketSurumId)
                    .OrderByDescending(d => d.OlusturmaTarihi)
                    .ToListAsync(ct); //GetDavetlerAsync neden Where kullanıyor: Tüm davetleri değil,
                                      //sadece belirli bir anket sürümüne ait olanları istiyoruz —
                                      //Detay sayfasında "bu anket için hangi davetler üretilmiş" diye göstereceğiz.
    public async Task<Davet?> GetDavetByTokenAsync(string token, CancellationToken ct = default)   
    => await _db.Davetler.FirstOrDefaultAsync(d => d.Token == token, ct);                       

    public async Task AddYanitOturumuAsync(YanitOturumu oturum, CancellationToken ct = default)     
        => await _db.YanitOturumlari.AddAsync(oturum, ct);    //GetDavetByTokenAsync neden gerekli: Hasta QR kodu okuttuğunda
                                                              //elinde sadece bir token (metin) var,
                                                              //Davet'in ID'sini bilmiyor — bu yüzden ID'ye göre değil,
                                                              //token'a göre arama yapan ayrı bir metoda ihtiyacımız oldu.                                        
    public async Task<IReadOnlyList<YanitKaydi>> GetYanitlarAsync(long anketSurumId, CancellationToken ct = default)
    {
        //O oturumun hangi davete, o davetin de hangi ankete ait olduğunu bulmak için üç tabloyu zincirleme birleştiriyoruz —
        //draw.io diyagramındaki YANIT → YANIT_OTURUMU → DAVET → ANKET_SURUM zincirini SQL'e çeviriyoruz.
        var query =
            from y in _db.Yanitlar
            join yo in _db.YanitOturumlari on EF.Property<long>(y, "YANIT_OTURUMU_ID") equals yo.Id
            join d in _db.Davetler on yo.DavetId equals d.Id
            join s in _db.Sorular on y.SoruId equals s.Id
            join sc in _db.SoruSecenekleri on y.SecenekId equals sc.Id into scGroup
            from sc in scGroup.DefaultIfEmpty()
            where d.AnketSurumId == anketSurumId
            select new YanitKaydi(yo.Id, s.Id, s.Metin, s.Tip.ToString(), sc != null ? (int?)sc.Agirlik : null, y.MetinDeger);
        //Sonucu, Domain'de tanımladığımız taşıyıcı tipe döküyor.

        return await query.ToListAsync(ct); //Normal join kullansaydık, açık uçlu cevaplar sonuçtan tamamen kaybolurdu.
                                            //DefaultIfEmpty(), "eşleşme yoksa da satırı at, sadece sc'yi null yap" diyor.
    }

    public async Task AddAksiyonAsync(Aksiyon aksiyon, CancellationToken ct = default)
    => await _db.Aksiyonlar.AddAsync(aksiyon, ct);

    public async Task<IReadOnlyList<Aksiyon>> GetAksiyonlarAsync(CancellationToken ct = default)
        => await _db.Aksiyonlar.AsNoTracking().OrderByDescending(a => a.OlusturmaTarihi).ToListAsync(ct);

    public Task<Aksiyon?> GetAksiyonByIdAsync(long id, CancellationToken ct = default)
        => _db.Aksiyonlar.FirstOrDefaultAsync(a => a.Id == id, ct);
    public async Task<string?> GetSoruMetniAsync(long soruId, CancellationToken ct = default)
    => await _db.Sorular.Where(s => s.Id == soruId).Select(s => s.Metin).FirstOrDefaultAsync(ct);
    public async Task<IReadOnlyList<DenetimIziKaydi>> GetDenetimIzleriAsync(CancellationToken ct = default)
    {
        var query =
            from d in _db.DenetimIzleri.AsNoTracking()
            join k in _db.Kullanicilar.AsNoTracking() on d.KullaniciId equals k.Id into kGroup
            from k in kGroup.DefaultIfEmpty() //Rapor sorgusunda da kullanmıştık, KullaniciId boş olabilir
                                              //(sistem tarafından tetiklenen bir işlem),
                                              //o zaman k de null olur, KullaniciAdi de null kalır — hata vermeden.
            orderby d.Zaman descending
            select new DenetimIziKaydi(d.Id, d.KullaniciId, k != null ? k.KullaniciAdi : null, d.Islem, d.HedefTablo, d.HedefId, d.Zaman);

        return await query.ToListAsync(ct);
    }
    public async Task SilAsync(long anketSurumId, CancellationToken ct = default)
    {
        var surum = await _db.AnketSurumleri
            .Include(a => a.Bolumler)
                .ThenInclude(b => b.Sorular)
                    .ThenInclude(s => s.Secenekler)
            .FirstOrDefaultAsync(a => a.Id == anketSurumId, ct);

        if (surum == null) return;

        foreach (var bolum in surum.Bolumler)
        {
            foreach (var soru in bolum.Sorular)
            {
                _db.SoruSecenekleri.RemoveRange(soru.Secenekler);
            }
            _db.Sorular.RemoveRange(bolum.Sorular);
        }
        _db.Bolumler.RemoveRange(surum.Bolumler);
        _db.AnketSurumleri.Remove(surum);

        await _db.SaveChangesAsync(ct);
    }
    public Task<Davet?> GetDavetByIdAsync(long davetId, CancellationToken ct = default)
    => _db.Davetler.FirstOrDefaultAsync(d => d.Id == davetId, ct);

    public async Task DavetSilAsync(long davetId, CancellationToken ct = default)
    {
        var davet = await _db.Davetler.FirstOrDefaultAsync(d => d.Id == davetId, ct);
        if (davet != null)
            _db.Davetler.Remove(davet);

        await _db.SaveChangesAsync(ct);
    }
    public async Task<AnketOzetBilgi?> GetAnketBilgisiAsync(long anketId, CancellationToken ct = default)
    {
        var anket = await _db.Anketler.AsNoTracking().FirstOrDefaultAsync(a => a.Id == anketId, ct);
        return anket == null ? null : new AnketOzetBilgi(anket.Ad, anket.HizmetTuru);
    }
    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
   
}