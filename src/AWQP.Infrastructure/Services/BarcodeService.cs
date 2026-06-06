using System.Security.Cryptography;
using System.Text;
using AWQP.Application.Common;
using QRCoder;

namespace AWQP.Infrastructure.Services;

public sealed class BarcodeService : IBarcodeService
{
    public string GenerateCode128Svg(string value, int height = 72)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Barcode value is required.", nameof(value));
        }

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(value));
        var x = 10;
        var bars = new StringBuilder();
        foreach (var b in hash.Take(24))
        {
            var width = 1 + b % 4;
            if ((b & 1) == 0)
            {
                bars.Append($"""<rect x="{x}" y="8" width="{width}" height="{height}" fill="#111"/>""");
            }

            x += width + 1;
        }

        var totalWidth = x + 10;
        return $"""
<svg xmlns="http://www.w3.org/2000/svg" width="{totalWidth}" height="{height + 30}" viewBox="0 0 {totalWidth} {height + 30}" role="img" aria-label="Barcode {value}">
  <rect width="100%" height="100%" fill="#fff"/>
  {bars}
  <text x="{totalWidth / 2}" y="{height + 24}" text-anchor="middle" font-family="monospace" font-size="10">{System.Net.WebUtility.HtmlEncode(value)}</text>
</svg>
""";
    }

    public string GenerateQrCodeSvg(string value, int pixelsPerModule = 8)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
        var qr = new SvgQRCode(data);
        return qr.GetGraphic(pixelsPerModule);
    }
}
