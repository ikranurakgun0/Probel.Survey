namespace Probel.Survey.Domain.Services;

public interface IDenetimKaydedici
{
    Task KaydetAsync(long? kullaniciId, string islem, string? hedefTablo = null, long? hedefId = null, CancellationToken ct = default);
}