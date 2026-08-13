using System.Security.Cryptography;
using Probel.Survey.Domain.Services;

namespace Probel.Survey.Infrastructure.Services;

public class TokenUretici : ITokenUretici
{
    public string Uret()
    {
        Span<byte> bytes = stackalloc byte[24];
        RandomNumberGenerator.Fill(bytes);

        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }//Bu satır, "Token sütunu asla tekrar edemez"
     //kısıtını EF Core'a da bildiriyor
     //şemandaki UNIQUE kısıtının kod tarafındaki karşılığı.
}