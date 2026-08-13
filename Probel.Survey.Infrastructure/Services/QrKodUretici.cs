using QRCoder;

namespace Probel.Survey.Infrastructure.Services;

public interface IQrKodUretici
{
    byte[] Uret(string metin);
}

public class QrKodUretici : IQrKodUretici
{
    public byte[] Uret(string metin)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(metin, QRCodeGenerator.ECCLevel.Q);
        using var qr = new PngByteQRCode(data);
        return qr.GetGraphic(20);
    }
}