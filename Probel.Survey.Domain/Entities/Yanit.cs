namespace Probel.Survey.Domain.Entities;

public class Yanit
{
    public long Id { get; private set; }
    public long SoruId { get; private set; }
    public long? SecenekId { get; private set; }
    public string? MetinDeger { get; private set; }

    private Yanit() { }

    public Yanit(long soruId, long? secenekId, string? metinDeger)
    {
        SoruId = soruId;
        SecenekId = secenekId;
        MetinDeger = metinDeger;
    }
}