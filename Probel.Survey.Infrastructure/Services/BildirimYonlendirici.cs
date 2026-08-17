using Probel.Survey.Domain.Services;

namespace Probel.Survey.Infrastructure.Services;

public class BildirimYonlendirici : IBildirimGonderici
{
    private readonly GmailBildirimGonderici _eposta;
    private readonly IletiMerkeziBildirimGonderici _sms;

    public BildirimYonlendirici(GmailBildirimGonderici eposta, IletiMerkeziBildirimGonderici sms)
    {
        _eposta = eposta;
        _sms = sms;
    }

    public Task<bool> GonderAsync(string hedef, string kanal, string mesaj, CancellationToken ct = default)
        => kanal switch
        {
            "EPOSTA" => _eposta.GonderAsync(hedef, kanal, mesaj, ct),
            "SMS" => _sms.GonderAsync(hedef, kanal, mesaj, ct),
            _ => Task.FromResult(false)
        };
}
