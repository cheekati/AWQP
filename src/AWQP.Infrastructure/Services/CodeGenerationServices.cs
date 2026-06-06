using AWQP.Application.Interfaces;
using System.IO.Compression;
using System.Text;
using QRCoder;
using ZXing;
using ZXing.Common;
using ZXing.Rendering;

namespace AWQP.Infrastructure.Services;

public sealed class QrCodeService : IQrCodeService
{
    public byte[] GenerateQrPng(string value)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(value, QRCodeGenerator.ECCLevel.Q);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(20);
    }
}

public sealed class BarcodeService : IBarcodeService
{
    public byte[] GenerateCode128Png(string value)
    {
        var writer = new BarcodeWriterPixelData
        {
            Format = BarcodeFormat.CODE_128,
            Options = new EncodingOptions { Height = 120, Width = 480, Margin = 8 }
        };
        var pixelData = writer.Write(value);
        return PngEncoder.EncodeRgba(pixelData.Pixels, pixelData.Width, pixelData.Height);
    }
}

internal static class PngEncoder
{
    private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };

    public static byte[] EncodeRgba(byte[] rgba, int width, int height)
    {
        using var raw = new MemoryStream();
        for (var y = 0; y < height; y++)
        {
            raw.WriteByte(0); // PNG scanline filter type 0.
            raw.Write(rgba, y * width * 4, width * 4);
        }

        using var compressed = new MemoryStream();
        using (var zlib = new ZLibStream(compressed, CompressionLevel.Optimal, leaveOpen: true))
        {
            raw.Position = 0;
            raw.CopyTo(zlib);
        }

        using var png = new MemoryStream();
        png.Write(Signature);
        WriteChunk(png, "IHDR", BuildHeader(width, height));
        WriteChunk(png, "IDAT", compressed.ToArray());
        WriteChunk(png, "IEND", Array.Empty<byte>());
        return png.ToArray();
    }

    private static byte[] BuildHeader(int width, int height)
    {
        using var header = new MemoryStream();
        WriteInt32(header, width);
        WriteInt32(header, height);
        header.WriteByte(8); // bit depth
        header.WriteByte(6); // RGBA
        header.WriteByte(0);
        header.WriteByte(0);
        header.WriteByte(0);
        return header.ToArray();
    }

    private static void WriteChunk(Stream stream, string type, byte[] data)
    {
        WriteInt32(stream, data.Length);
        var typeBytes = Encoding.ASCII.GetBytes(type);
        stream.Write(typeBytes);
        stream.Write(data);
        var crcInput = typeBytes.Concat(data).ToArray();
        WriteInt32(stream, Crc32(crcInput));
    }

    private static void WriteInt32(Stream stream, int value)
    {
        stream.WriteByte((byte)((value >> 24) & 0xff));
        stream.WriteByte((byte)((value >> 16) & 0xff));
        stream.WriteByte((byte)((value >> 8) & 0xff));
        stream.WriteByte((byte)(value & 0xff));
    }

    private static int Crc32(byte[] bytes)
    {
        const uint polynomial = 0xedb88320u;
        var crc = 0xffffffffu;
        foreach (var b in bytes)
        {
            crc ^= b;
            for (var i = 0; i < 8; i++)
            {
                crc = (crc & 1) == 1 ? (crc >> 1) ^ polynomial : crc >> 1;
            }
        }
        return unchecked((int)(crc ^ 0xffffffffu));
    }
}
