using System.Net;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;
using Probel.Survey.Domain.Services;

namespace Probel.Survey.Infrastructure.Services;

public class IletiMerkeziBildirimGonderici : IBildirimGonderici
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    public IletiMerkeziBildirimGonderici(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<bool> GonderAsync(string hedef, string kanal, string mesaj, CancellationToken ct = default)
    {
        if (kanal != "SMS") return false;

        var key = _config["IletiMerkezi:ApiKey"];
        var hash = _config["IletiMerkezi:ApiHash"];
        var sender = _config["IletiMerkezi:Sender"];

        var telefon = hedef.TrimStart('0').Replace("+90", "").Replace(" ", "");

        var url = "https://api.iletimerkezi.com/v1/send-sms/get/"
            + $"?key={WebUtility.UrlEncode(key)}"
            + $"&hash={WebUtility.UrlEncode(hash)}"
            + $"&text={WebUtility.UrlEncode(mesaj)}"
            + $"&receipents={WebUtility.UrlEncode(telefon)}"
            + $"&sender={WebUtility.UrlEncode(sender)}"
            + "&iys=0";

        try
        {
            var response = await _http.GetAsync(url, ct);
            var body = await response.Content.ReadAsStringAsync(ct);
            var xml = XDocument.Parse(body);
            var kod = xml.Descendants("code").FirstOrDefault()?.Value;
            return kod == "200";
        }
        catch
        {
            return false;
        }
    }
}