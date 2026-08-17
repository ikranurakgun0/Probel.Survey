namespace Probel.Survey.Domain.Services;

public interface IBildirimGonderici
{
    Task<bool> GonderAsync(string hedef, string kanal, string mesaj, CancellationToken ct = default);
}