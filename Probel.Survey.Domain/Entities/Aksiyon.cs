namespace Probel.Survey.Domain.Entities;

public enum AksiyonDurumu
{
    Acik = 0,
    DevamEdiyor = 1,
    Kapandi = 2
}

public class Aksiyon
{
    public long Id { get; private set; }
    public long SoruId { get; private set; }
    public long? SorumluId { get; private set; }
    public string Aciklama { get; private set; } = null!;
    public DateTime? HedefTarih { get; private set; }
    public AksiyonDurumu Durum { get; private set; }
    public DateTime OlusturmaTarihi { get; private set; }

    private Aksiyon() { }

    public Aksiyon(long soruId, string aciklama, DateTime? hedefTarih)
    {
        if (string.IsNullOrWhiteSpace(aciklama))
            throw new ArgumentException("Aksiyon açıklaması boş olamaz.");

        SoruId = soruId;
        Aciklama = aciklama;
        HedefTarih = hedefTarih;
        Durum = AksiyonDurumu.Acik;
        OlusturmaTarihi = DateTime.UtcNow;
    }

    public void DurumGuncelle(AksiyonDurumu yeniDurum) => Durum = yeniDurum;
}