namespace Probel.Survey.Domain.Entities;

public class Anket
{
    public long Id { get; private set; }
    public string Ad { get; private set; } = null!;
    public string? HizmetTuru { get; private set; }

    private Anket() { }

    public Anket(string ad, string? hizmetTuru)
    {
        if (string.IsNullOrWhiteSpace(ad))
            throw new ArgumentException("Anket adı boş olamaz.");

        Ad = ad;
        HizmetTuru = hizmetTuru;
    }
}