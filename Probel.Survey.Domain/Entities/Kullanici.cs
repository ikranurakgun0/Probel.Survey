namespace Probel.Survey.Domain.Entities;

public class Kullanici
{
    public long Id { get; private set; }
    public string KullaniciAdi { get; private set; } = null!;
    public string SifreHash { get; private set; } = null!;
    public string AdSoyad { get; private set; } = null!;
    public bool AktifMi { get; private set; }
    public bool YoneticiMi { get; private set; }

    private Kullanici() { }

    public Kullanici(string kullaniciAdi, string sifreHash, string adSoyad, bool yoneticiMi = false)
    {
        if (string.IsNullOrWhiteSpace(kullaniciAdi))
            throw new ArgumentException("Kullanıcı adı boş olamaz.");

        KullaniciAdi = kullaniciAdi;
        SifreHash = sifreHash;
        AdSoyad = adSoyad;
        AktifMi = true;
        YoneticiMi = yoneticiMi;
    }

    public void SifreGuncelle(string yeniHash) => SifreHash = yeniHash;
    //Not: Şemandaki KULLANICI tablosu zaten hazırdı (hatırlarsan AD_SOYAD
    //orada sistem kullanıcısı içindi, hastayla ilgisiz),
    //sadece C# tarafında hiç entity yazmamıştık.
    public void Pasiflestir()
    {
        if (YoneticiMi)
            throw new InvalidOperationException("Yönetici hesabı pasifleştirilemez.");

        AktifMi = false;
    }
    public void Aktiflestir() => AktifMi = true;
}