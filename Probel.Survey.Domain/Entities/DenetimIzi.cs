namespace Probel.Survey.Domain.Entities;

public class DenetimIzi
{
    public long Id { get; private set; }
    public long? KullaniciId { get; private set; }
    public string Islem { get; private set; } = null!;
    public string? HedefTablo { get; private set; }
    public long? HedefId { get; private set; }
    public DateTime Zaman { get; private set; }

    private DenetimIzi() { }

    public DenetimIzi(long? kullaniciId, string islem, string? hedefTablo, long? hedefId)
    {
        if (string.IsNullOrWhiteSpace(islem))
            throw new ArgumentException("İşlem adı boş olamaz.");

        KullaniciId = kullaniciId;
        Islem = islem;
        HedefTablo = hedefTablo;
        HedefId = hedefId;
        Zaman = DateTime.UtcNow;
    }
}