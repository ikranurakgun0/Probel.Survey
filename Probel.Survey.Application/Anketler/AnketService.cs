using Microsoft.Extensions.Configuration;
using Probel.Survey.Domain.Entities;
using Probel.Survey.Domain.Repositories;
using Probel.Survey.Domain.Services;
using Microsoft.Extensions.Configuration;

namespace Probel.Survey.Application.Anketler;

public class AnketService : IAnketService
{
    private readonly IAnketRepository _repository;
    private readonly ITokenUretici _tokenUretici;
    private readonly IDenetimKaydedici _denetim;
    private readonly IBildirimGonderici _bildirim;
    private readonly IConfiguration _config;


    public AnketService(IAnketRepository repository, ITokenUretici tokenUretici, IDenetimKaydedici denetim, IBildirimGonderici bildirim, IConfiguration config)
    {
        _repository = repository;
        _tokenUretici = tokenUretici;
        _denetim = denetim;
        _bildirim = bildirim;
        _config = config;
        
    }

    public async Task<IReadOnlyList<AnketListeDto>> GetAllAsync(CancellationToken ct = default)
    {
        var surumler = await _repository.GetAllAsync(ct);
        return surumler
            .Select(s => new AnketListeDto(s.Id, s.SurumNo, s.Durum, s.AnketAdi, s.HizmetTuru))
            .ToList();  //AnketService, IAnketRepository'yi çağırıp
                        //gelen AnketSurum nesnelerini AnketListeDto'ya çeviriyor
                        //Neden Domain entity'sini doğrudan View'a göndermiyoruz?
                        //Çünkü AnketSurum nesnesinin içinde Yayinla() gibi metotlar, iş kuralları var —
                        //bunları View'a sızdırmak istemiyoruz, ayrıca ileride ekranın ihtiyacı değişirse
                        //(örneğin "Başlık" göstermek istersek) Domain'e dokunmadan sadece DTO'yu güncelleriz.

    }
    public async Task<long> CreateAsync(string ad, string? hizmetTuru, CancellationToken ct = default)
    {
        var anket = new Anket(ad, hizmetTuru);
        await _repository.AddAnketAsync(anket, ct); // 1. kayıt — anket.Id burada doluyor
        await _repository.SaveChangesAsync(ct);

        var ilkSurum = new AnketSurum(anket.Id, 1);
        await _repository.AddAsync(ilkSurum, ct); // 2. kayıt
        await _repository.SaveChangesAsync(ct);

        return ilkSurum.Id; //Burada tek bir "anket oluştur" isteğinin aslında iki ayrı Oracle tablosuna
                            //yazma işlemi olduğunu görüyorsun:
                            //önce ANKET tablosuna bir satır (isim),
                            //sonra ANKET_SURUM tablosuna bir satır (sürüm 1, taslak durumda).
                            //İki ayrı SaveChangesAsync çağırmamızın sebebi şuydu: ikinci satırı yazabilmek için,
                            //birinci satırın Oracle tarafından üretilen
                            //ID'sine (IDENTITY sütunu, hatırlarsan) ihtiyacımız var
                            //— o ID, ancak ilk kayıt gerçekten Oracle'a gidince belli oluyor.
    }
    public async Task YayinlaAsync(long id, long? kullaniciId, CancellationToken ct = default)
    {
        var surum = await _repository.GetByIdAsync(id, ct) //GetByIdAsync(id, ct) → Oracle'dan o anketi çek.
                    ?? throw new KeyNotFoundException("Anket sürümü bulunamadı.");//?? operatörü (null-coalescing) şunu söylüyor:
                                                                                  //"Solundaki değer null değilse onu kullan, null ise sağındakini çalıştır."
                                                                                  //Burada sağ taraf bir hata fırlatma (throw) olduğu için, cümle şu anlama geliyor:
                                                                                  //"Oracle'dan bir AnketSurum getir. Eğer öyle bir kayıt yoksa (null döndüyse),
                                                                                  //hemen hata fırlat ve dur. Varsa, onu surum değişkenine ata."
                        //Servis, kararı Domain'e bırakır	YayinlaAsync içinde surum.Yayinla() çağrısı
        surum.Yayinla();//Servis, "yayınlanabilir mi" kararını kendisi vermiyor.
                        //Bu kararı tamamen Domain'deki AnketSurum.Yayinla() metoduna devrediyor.
                        //Servis sadece "sen karar ver" diyor, kuralın kendisi burada değil.
        await _repository.SaveChangesAsync(ct);//Eğer Yayinla() hata fırlatmadan geçtiyse
                                               //(yani gerçekten sorusu varsa), değişikliği Oracle'a yaz.
        await _denetim.KaydetAsync(kullaniciId, "ANKET_YAYINLA", "ANKET_SURUM", id, ct);
    }
    public async Task<AnketDetayDto> GetDetayAsync(long id, CancellationToken ct = default)
    {
        var surum = await _repository.GetByIdAsync(id, ct)
                    ?? throw new KeyNotFoundException("Anket sürümü bulunamadı.");

        var anketBilgi = await _repository.GetAnketBilgisiAsync(surum.AnketId, ct);

        return new AnketDetayDto(
            surum.Id,
            surum.SurumNo,
            surum.Durum.ToString(),
            anketBilgi?.Ad ?? "(bilinmiyor)",
            anketBilgi?.HizmetTuru,
            surum.Bolumler.Select(b => new BolumDetayDto(
                b.Id, b.Ad, b.Sira,
                b.Sorular.Select(s => new SoruDetayDto(s.Id, s.Metin, s.Tip.ToString(), s.ZorunluMu)).ToList()
            )).ToList()
        );
    }//Ne işe yarıyor: Oracle'dan gelen AnketSurum
     //nesnesini (bölümleri ve sorularıyla birlikte),
     //View'a gönderilecek AnketDetayDto'ya çeviriyor. İçteki .Select(...) zincirleri,
     //"her bölüm için bir BolumDetayDto üret,
     //onun içinde de her soru için bir SoruDetayDto üret" diyor — iç içe dönüşüm.

    public async Task BolumEkleAsync(long anketSurumId, string ad, CancellationToken ct = default)
    {
        var surum = await _repository.GetByIdAsync(anketSurumId, ct)
                    ?? throw new KeyNotFoundException("Anket sürümü bulunamadı.");

        var siradakiSira = surum.Bolumler.Count + 1;
        surum.BolumEkle(new Bolum(ad, siradakiSira));//Burada Oracle'a doğrudan INSERT INTO BOLUM yazmıyoruz. Bunun yerine: anketi Oracle'dan çekiyoruz
                                                     //(bu, EF Core'un "takip ettiği" canlı bir nesne), surum.BolumEkle(...)
                                                     //ile Domain'in kendi metodunu çağırıyoruz (hatırlarsan bu metotta
                                                     //"yalnızca taslak ankete bölüm eklenebilir" kontrolü vardı), sonra SaveChangesAsync diyoruz.
                                                     //EF Core, nesnede ne değiştiğini kendisi anlıyor
                                                     //ve arka planda doğru INSERT komutunu otomatik üretiyor — biz hiç SQL yazmadık.
        await _repository.SaveChangesAsync(ct);
    }
    public async Task SoruEkleAsync(long anketSurumId, long bolumId, string metin, SoruTipi tip, bool zorunluMu, CancellationToken ct = default)
    {
        var surum = await _repository.GetByIdAsync(anketSurumId, ct)
                    ?? throw new KeyNotFoundException("Anket sürümü bulunamadı.");

        var bolum = surum.Bolumler.FirstOrDefault(b => b.Id == bolumId)
                    ?? throw new KeyNotFoundException("Bölüm bulunamadı.");

        var siradakiSira = bolum.Sorular.Count + 1;
        var soru = new Soru(metin, tip, zorunluMu, siradakiSira);

        if (tip == SoruTipi.Olcek5)
        {
            soru.SecenekEkle(new SoruSecenek("Tamamen katılıyorum", 4, 1));
            soru.SecenekEkle(new SoruSecenek("Katılıyorum", 3, 2));
            soru.SecenekEkle(new SoruSecenek("Kararsızım", 2, 3));
            soru.SecenekEkle(new SoruSecenek("Katılmıyorum", 1, 4));
            soru.SecenekEkle(new SoruSecenek("Kesinlikle katılmıyorum", 0, 5));
        }

        bolum.SoruEkle(soru);
        await _repository.SaveChangesAsync(ct);
    }

    
    public async Task<string> DavetOlusturAsync(long anketSurumId, CancellationToken ct = default)
    {
        var surum = await _repository.GetByIdAsync(anketSurumId, ct)
                    ?? throw new KeyNotFoundException("Anket sürümü bulunamadı.");

        if (surum.Durum != AnketDurumu.Yayinda) 
            throw new InvalidOperationException("Yalnızca yayındaki anketler için davet oluşturulabilir.");//Bu bir iş kuralı, ama Domain'de değil burada —
                                                                                                           //çünkü iki farklı entity'yi
                                                                                                           //(Anket'in durumu + Davet oluşturma) ilgilendiriyor,
                                                                                                           //tek bir nesnenin kendi bütünlüğü değil.

        var token = _tokenUretici.Uret();
        var davet = new Davet(anketSurumId, token);
        await _repository.AddDavetAsync(davet, ct);
        await _repository.SaveChangesAsync(ct);

        return token; //Verilen anket ID'sini Oracle'dan çekiyor,
                      //ankete yayında olup olmadığını kontrol ediyor
                      //(değilse hata), token üretiyor,
                      //yeni bir Davet nesnesi oluşturup Oracle'a kaydediyor,
                      //üretilen token'ı geri döndürüyor.
    }
    public async Task<IReadOnlyList<DavetDto>> GetDavetlerAsync(long anketSurumId, CancellationToken ct = default) //IReadOnlyList<DavetDto> — bir liste,
                                                                                                                   //içinde DavetDto tipinde nesneler var.
                                                                                                                   //Yani bu metot çağrıldığında sana bir
                                                                                                                   //davet listesi dönecek.
    {
        var davetler = await _repository.GetDavetlerAsync(anketSurumId, ct);
        return davetler
            .Select(d => new DavetDto(d.Id, d.Token, d.Durum.ToString(), d.SonGecerlilik))
            .ToList();
    }//Belirli bir ankete ait tüm davetleri Oracle'dan çekip,
     //ekrana gönderilecek sade DavetDto listesine çeviriyor —
     //tıpkı GetAllAsync'in yaptığı gibi, sadece Davet için.

    public async Task<AnketDoldurmaDto> DoldurmaBaslatAsync(string token, CancellationToken ct = default)
    {
        var davet = await _repository.GetDavetByTokenAsync(token, ct)
                    ?? throw new KeyNotFoundException("Geçersiz bağlantı.");

        if (davet.Durum == DavetDurumu.Kullanildi)
            throw new InvalidOperationException("Bu anket daha önce doldurulmuş.");

        if (!davet.GecerliMi())
            throw new InvalidOperationException("Bu bağlantının süresi dolmuş.");

        var surum = await _repository.GetByIdAsync(davet.AnketSurumId, ct)
                    ?? throw new KeyNotFoundException("Anket bulunamadı.");

        return new AnketDoldurmaDto(
            surum.Id,  //Bölümlerin ve soruların biz onları eklerken belirlediğimiz sırada görünmesini garanti ediyor.
            token,     //OrderBy diyerek veritabanı Oracle'dan gelen verilerin sırasına güvenmeden kendimiz sıralıyoruz kesin olsun diye.
            surum.Bolumler.OrderBy(b => b.Sira).Select(b => new BolumDoldurmaDto(
                b.Id, b.Ad,
                b.Sorular.OrderBy(s => s.Sira).Select(s => new SoruDoldurmaDto(
                    s.Id, s.Metin, s.Tip.ToString(), s.ZorunluMu,
                    s.Secenekler.OrderBy(sc => sc.Sira).Select(sc => new SecenekDto(sc.Id, sc.Metin)).ToList()
                )).ToList()
            )).ToList()
        );
    }

    public async Task GonderAsync(string token, List<CevapGirisi> cevaplar, string? cinsiyet, string? yasAraligi, CancellationToken ct = default)
    {
        var davet = await _repository.GetDavetByTokenAsync(token, ct)
                    ?? throw new KeyNotFoundException("Geçersiz bağlantı.");
        var surum = await _repository.GetByIdAsync(davet.AnketSurumId, ct)
                ?? throw new KeyNotFoundException("Anket bulunamadı.");

       
        var zorunluSoruIdleri = surum.Bolumler
            .SelectMany(b => b.Sorular)
            .Where(s => s.ZorunluMu)
            .Select(s => s.Id)
            .ToList();

        var cevaplananSoruIdleri = cevaplar
            .Where(c => c.SecenekId != null || !string.IsNullOrWhiteSpace(c.MetinDeger))
            .Select(c => c.SoruId)
            .ToHashSet();

        if (zorunluSoruIdleri.Any(id => !cevaplananSoruIdleri.Contains(id)))
            throw new InvalidOperationException("Lütfen tüm zorunlu (*) soruları cevaplayın.");

        davet.Kullan();  // Domain kuralı burada tetikleniyor.

        var oturum = new YanitOturumu(davet.Id);

        if (cinsiyet != null || yasAraligi != null)
            oturum.DemografiEkle(new Demografi(null, cinsiyet, yasAraligi, null));

        foreach (var cevap in cevaplar)
        {
            if (cevap.SecenekId == null && string.IsNullOrWhiteSpace(cevap.MetinDeger))
                continue; // boş cevapları atla

            oturum.YanitEkle(new Yanit(cevap.SoruId, cevap.SecenekId, cevap.MetinDeger));
        }

        oturum.Tamamla();

        await _repository.AddYanitOturumuAsync(oturum, ct);
        await _repository.SaveChangesAsync(ct);
    }
    public async Task<AnketRaporDto> GetRaporAsync(long anketSurumId, CancellationToken ct = default)
    {
        var yanitlar = await _repository.GetYanitlarAsync(anketSurumId, ct);
        var olcekYanitlari = yanitlar
            .Where(y => y.SoruTipi == "Olcek5" && y.Agirlik != null)
            .ToList();
        var soruSonuclari = olcekYanitlari
            .GroupBy(y => new { y.SoruId, y.SoruMetni }) //SQL'deki GROUP BY'ın LINQ karşılığı.
            .Select(g => new SoruRaporDto(
                g.Key.SoruId,
                g.Key.SoruMetni,
                g.Count(),
                Math.Round((double)g.Sum(x => x.Agirlik!.Value) / (g.Count() * 4) * 100, 1),
                false, //DusukPerformans
                0 //AcikActivasyonSayisi

            ))
            .OrderBy(s => s.KarsilanmaOrani)
            .ToList();

        var aksiyonlar = await _repository.GetAksiyonlarAsync(ct);

        soruSonuclari = soruSonuclari.Select(s => s with
        {
            DusukPerformans = s.KarsilanmaOrani < 50,
            AcikAksiyonSayisi = aksiyonlar.Count(a => a.SoruId == s.SoruId && a.Durum != AksiyonDurumu.Kapandi)
        }).ToList();

        var genelSkor = soruSonuclari.Any()
            ? Math.Round(soruSonuclari.Average(s => s.KarsilanmaOrani), 1)
            : 0;
        var acikUcluYorumlar = yanitlar
            .Where(y => y.SoruTipi == "Metin" && !string.IsNullOrWhiteSpace(y.MetinDeger))
            .Select(y => y.MetinDeger!)
            .ToList();
        var toplamKatilim = yanitlar
            .Select(y => y.YanitOturumuId)
            .Distinct() //her hasta birden çok soruya cevap veriyor (birden çok YanitKaydi satırı üretiyor),
                        //gerçek katılımcı sayısını bulmak için Distinct() kullanarak tekilleştiriyoruz.
            .Count();
        return new AnketRaporDto(anketSurumId, toplamKatilim, genelSkor, soruSonuclari, acikUcluYorumlar);
    }
    public async Task AksiyonAcAsync(long soruId, string aciklama, DateTime? hedefTarih, CancellationToken ct = default)
    {
        var aksiyon = new Aksiyon(soruId, aciklama, hedefTarih);
        await _repository.AddAksiyonAsync(aksiyon, ct);
        await _repository.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<AksiyonListeDto>> GetAksiyonlarAsync(CancellationToken ct = default)
    {
        var aksiyonlar = await _repository.GetAksiyonlarAsync(ct);
        var sonuc = new List<AksiyonListeDto>();

        foreach (var a in aksiyonlar)
        {
            var soru = await _repository.GetSoruMetniAsync(a.SoruId, ct);
            sonuc.Add(new AksiyonListeDto(a.Id, soru ?? "(silinmiş soru)", a.Aciklama, a.HedefTarih, a.Durum.ToString(), a.OlusturmaTarihi));
        }

        return sonuc;
    }

    public async Task AksiyonDurumGuncelleAsync(long aksiyonId, string yeniDurum, long? kullaniciId, CancellationToken ct = default)
    {
        var aksiyon = await _repository.GetAksiyonByIdAsync(aksiyonId, ct)
                      ?? throw new KeyNotFoundException("Aksiyon bulunamadı.");

        if (!Enum.TryParse<AksiyonDurumu>(yeniDurum, out var durum))
            throw new ArgumentException("Geçersiz durum.");

        aksiyon.DurumGuncelle(durum);
        await _repository.SaveChangesAsync(ct);

        await _denetim.KaydetAsync(kullaniciId, "AKSIYON_DURUM_DEGISTIR", "AKSIYON", aksiyonId, ct);
    }
    public async Task<IReadOnlyList<DenetimIziDto>> GetDenetimIzleriAsync(CancellationToken ct = default)
    {
        var kayitlar = await _repository.GetDenetimIzleriAsync(ct);
        return kayitlar
            .Select(d => new DenetimIziDto(d.Id, d.KullaniciId, d.KullaniciAdi, d.Islem, d.HedefTablo, d.HedefId, d.Zaman))
            .ToList();
    }
    public async Task<KarsilastirmaRaporuDto> GetKarsilastirmaRaporuAsync(CancellationToken ct = default)
    {
        var surumler = await _repository.GetAllAsync(ct);
        var yayindakiler = surumler.Where(s => s.Durum == "Yayinda" && s.HizmetTuru != null).ToList();

        var gruplar = yayindakiler.GroupBy(s => s.HizmetTuru!);
        var sonuc = new List<HizmetTuruKarsilastirmaDto>();

        foreach (var grup in gruplar)
        {
            var tumYanitlar = new List<YanitKaydi>();
            foreach (var surum in grup)
            {
                var yanitlar = await _repository.GetYanitlarAsync(surum.Id, ct);
                tumYanitlar.AddRange(yanitlar);
            }

            var olcekYanitlari = tumYanitlar.Where(y => y.SoruTipi == "Olcek5" && y.Agirlik != null).ToList();

            double genelSkor = 0;
            if (olcekYanitlari.Any())
            {
                var soruBazinda = olcekYanitlari
                    .GroupBy(y => y.SoruId)
                    .Select(g => Math.Round((double)g.Sum(x => x.Agirlik!.Value) / (g.Count() * 4) * 100, 1));
                genelSkor = Math.Round(soruBazinda.Average(), 1);
            }

            var toplamKatilim = tumYanitlar.Select(y => y.YanitOturumuId).Distinct().Count();

            sonuc.Add(new HizmetTuruKarsilastirmaDto(grup.Key, toplamKatilim, genelSkor));
        }

        return new KarsilastirmaRaporuDto(sonuc.OrderByDescending(s => s.GenelSkor).ToList());
    }
    

   
    public async Task ArsivleAsync(long id, long? kullaniciId, CancellationToken ct = default)
    {
        var surum = await _repository.GetByIdAsync(id, ct)
                    ?? throw new KeyNotFoundException("Anket sürümü bulunamadı.");

        surum.Arsivle();
        await _repository.SaveChangesAsync(ct);

        await _denetim.KaydetAsync(kullaniciId, "ANKET_ARSIVLE", "ANKET_SURUM", id, ct);
    }
    public async Task AnketSilAsync(long anketSurumId, CancellationToken ct = default)
    {
        var surum = await _repository.GetByIdAsync(anketSurumId, ct)
                    ?? throw new KeyNotFoundException("Anket sürümü bulunamadı.");

        if (surum.Durum != AnketDurumu.Taslak)
            throw new InvalidOperationException("Yalnızca taslak durumundaki anketler silinebilir.");

        await _repository.SilAsync(anketSurumId, ct);
    }
    public async Task DavetSilAsync(long davetId, long? kullaniciId, CancellationToken ct = default)
    {
        var davet = await _repository.GetDavetByIdAsync(davetId, ct)
                    ?? throw new KeyNotFoundException("Davet bulunamadı.");

        if (davet.Durum == DavetDurumu.Kullanildi)
            throw new InvalidOperationException("Kullanılmış davetler silinemez.");

        await _repository.DavetSilAsync(davetId, ct);
        await _denetim.KaydetAsync(kullaniciId, "DAVET_SIL", "DAVET", davetId, ct);
    }
    public async Task<IReadOnlyList<TopluDavetSonucDto>> TopluDavetOlusturAsync(long anketSurumId, List<string> hedefler, long? kullaniciId, CancellationToken ct = default)
    {
        var surum = await _repository.GetByIdAsync(anketSurumId, ct)
                    ?? throw new KeyNotFoundException("Anket sürümü bulunamadı.");

        if (surum.Durum != AnketDurumu.Yayinda)
            throw new InvalidOperationException("Yalnızca yayındaki anketler için davet oluşturulabilir.");

        var publicBase = _config["PublicBaseUrl"];
        var sonuclar = new List<TopluDavetSonucDto>();

        foreach (var hedef in hedefler.Where(h => !string.IsNullOrWhiteSpace(h)))
        {
            var token = _tokenUretici.Uret();
            var davet = new Davet(anketSurumId, token);
            await _repository.AddDavetAsync(davet, ct);
            await _repository.SaveChangesAsync(ct);

            var link = !string.IsNullOrWhiteSpace(publicBase)
                ? $"{publicBase.TrimEnd('/')}/Anket/Doldur?token={token}"
                : $"/Anket/Doldur?token={token}";

            var mesaj = $"Hastanemizi tercih ettiginiz icin tesekkur ederiz. Deneyiminizi paylasmak icin: {link}";
            var basarili = await _bildirim.GonderAsync(hedef, "EPOSTA", mesaj, ct);

            sonuclar.Add(new TopluDavetSonucDto(MaskeleHedef(hedef), basarili, basarili ? token : null));
        }

        await _denetim.KaydetAsync(kullaniciId, "TOPLU_DAVET_GONDER", "ANKET_SURUM", anketSurumId, ct);
        return sonuclar;
    }

    private static string MaskeleHedef(string hedef)
    {
        var atIndex = hedef.IndexOf('@');
        if (atIndex <= 1) return "****";
        var yerel = hedef[..atIndex];
        var alanAdi = hedef[atIndex..];
        var gorunur = Math.Min(2, yerel.Length);
        return yerel[..gorunur] + new string('*', Math.Max(yerel.Length - gorunur, 3)) + alanAdi;
    }

}