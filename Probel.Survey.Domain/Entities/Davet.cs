namespace Probel.Survey.Domain.Entities;

public enum DavetDurumu
{
    Bekliyor = 0,
    Kullanildi = 1,
    SuresiDoldu = 2
}

public class Davet
{
    public long Id { get; private set; }
    public long AnketSurumId { get; private set; }
    public string Token { get; private set; } = null!;
    public DateTime OlusturmaTarihi { get; private set; }
    public DateTime SonGecerlilik { get; private set; }
    public DavetDurumu Durum { get; private set; }

    private Davet() { }

    public Davet(long anketSurumId, string token, int gecerlilikGunu = 7)
    {
        if (string.IsNullOrWhiteSpace(token))
            throw new ArgumentException("Token boş olamaz.");

        AnketSurumId = anketSurumId;
        Token = token;
        OlusturmaTarihi = DateTime.UtcNow;
        SonGecerlilik = DateTime.UtcNow.AddDays(gecerlilikGunu);
        Durum = DavetDurumu.Bekliyor;
    }

    public bool GecerliMi() => Durum == DavetDurumu.Bekliyor && SonGecerlilik > DateTime.UtcNow;

    public void Kullan()
    {
        if (Durum != DavetDurumu.Bekliyor)
            throw new InvalidOperationException("Bu davet zaten kullanılmış veya geçersiz.");

        if (SonGecerlilik <= DateTime.UtcNow)
            throw new InvalidOperationException("Davetin süresi dolmuş.");

        Durum = DavetDurumu.Kullanildi;
    }
}