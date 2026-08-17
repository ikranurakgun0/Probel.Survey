using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using Probel.Survey.Domain.Services;

namespace Probel.Survey.Infrastructure.Services;

public class GmailBildirimGonderici : IBildirimGonderici
{
    private readonly IConfiguration _config;
    public GmailBildirimGonderici(IConfiguration config) => _config = config;

    public async Task<bool> GonderAsync(string hedef, string kanal, string mesaj, CancellationToken ct = default)
    {
        if (kanal != "EPOSTA") return false;

        try
        {
            var adres = _config["Email:Adres"];
            var sifre = _config["Email:UygulamaSifresi"];

            var mail = new MimeMessage();
            mail.From.Add(MailboxAddress.Parse(adres));
            mail.To.Add(MailboxAddress.Parse(hedef));
            mail.Subject = "Hasta Memnuniyet Anketi";
            mail.Body = new TextPart("plain") { Text = mesaj };

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(adres, sifre, ct);
            await client.SendAsync(mail, ct);
            await client.DisconnectAsync(true, ct);

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine("MAIL HATASI: " + ex.ToString());
            return false;
        }
    }
}