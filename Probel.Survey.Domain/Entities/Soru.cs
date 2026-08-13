namespace Probel.Survey.Domain.Entities;

public enum SoruTipi
{
    Olcek5,
    TekSecim,
    CokSecim,
    EvetHayir,
    Metin,
    Puan10,
    Tarih
}

public class Soru
{
    public long Id { get; private set; }
    public string Metin { get; private set; } = null!;
    public SoruTipi Tip { get; private set; }
    public bool ZorunluMu { get; private set; }
    public int Sira { get; private set; }

    private readonly List<SoruSecenek> _secenekler = new(); 
    public IReadOnlyCollection<SoruSecenek> Secenekler => _secenekler.AsReadOnly();

    private Soru() { }

    public Soru(string metin, SoruTipi tip, bool zorunluMu, int sira)
    {
        if (string.IsNullOrWhiteSpace(metin))
            throw new ArgumentException("Soru metni boş olamaz.");

        Metin = metin;
        Tip = tip;
        ZorunluMu = zorunluMu;
        Sira = sira;
    }

    public void SecenekEkle(SoruSecenek secenek) => _secenekler.Add(secenek);
}

public class SoruSecenek
{
    public long Id { get; private set; }
    public string Metin { get; private set; } = null!;
    public int Agirlik { get; private set; }
    public int Sira { get; private set; }

    private SoruSecenek() { }

    public SoruSecenek(string metin, int agirlik, int sira)
    {
        Metin = metin;
        Agirlik = agirlik;
        Sira = sira;
    }
}