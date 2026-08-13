namespace Probel.Survey.Domain.Entities;

public class Demografi
{
    public long YanitOturumuId { get; private set; }
    public string? KatilimciTuru { get; private set; }
    public string? Cinsiyet { get; private set; }
    public string? YasAraligi { get; private set; }
    public string? EgitimDurumu { get; private set; }

    private Demografi() { }

    public Demografi(string? katilimciTuru, string? cinsiyet, string? yasAraligi, string? egitimDurumu)
    {
        KatilimciTuru = katilimciTuru;
        Cinsiyet = cinsiyet;
        YasAraligi = yasAraligi;
        EgitimDurumu = egitimDurumu;
    }
}