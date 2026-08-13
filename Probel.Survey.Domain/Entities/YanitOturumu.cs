namespace Probel.Survey.Domain.Entities;

public class YanitOturumu
{
    public long Id { get; private set; }
    public long DavetId { get; private set; }
    public DateTime BaslangicZamani { get; private set; }
    public DateTime? BitisZamani { get; private set; }
    public bool TamamlandiMi { get; private set; }
    public Demografi? Demografi { get; private set; }

    private readonly List<Yanit> _yanitlar = new();
    public IReadOnlyCollection<Yanit> Yanitlar => _yanitlar.AsReadOnly();

    private YanitOturumu() { }

    public YanitOturumu(long davetId)
    {
        DavetId = davetId;
        BaslangicZamani = DateTime.UtcNow;
        TamamlandiMi = false;
    }

    public void DemografiEkle(Demografi demografi) => Demografi = demografi;

    public void YanitEkle(Yanit yanit) => _yanitlar.Add(yanit);

    public void Tamamla()
    {
        TamamlandiMi = true;
        BitisZamani = DateTime.UtcNow;
    }
}