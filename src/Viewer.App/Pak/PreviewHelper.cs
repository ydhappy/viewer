using System.Drawing.Imaging;
using System.Text;

namespace Viewer.App.Pak;

public enum PreviewKind
{
    None,
    Text,
    Image,
    Hex,
    Special,
    Unsupported
}

public static class PreviewHelper
{
    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".html", ".htm", ".xml", ".ini", ".cfg", ".log", ".dat", ".csv", ".json"
    };

    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".png", ".bmp", ".jpg", ".jpeg", ".gif"
    };

    public static PreviewKind DetectKind(string fileName, byte[] data)
    {
        if (SpecialResourceAnalyzer.IsSpecialResource(fileName))
        {
            return PreviewKind.Special;
        }

        var extension = Path.GetExtension(fileName);

        if (TextExtensions.Contains(extension))
        {
            return PreviewKind.Text;
        }

        if (ImageExtensions.Contains(extension) || LooksLikeKnownBitmap(data))
        {
            return PreviewKind.Image;
        }

        if (data.Length <= 4096)
        {
            return PreviewKind.Hex;
        }

        return PreviewKind.Unsupported;
    }

    public static string DecodeText(byte[] data)
    {
        if (data.Length >= 3 && data[0] == 0xEF && data[1] == 0xBB && data[2] == 0xBF)
        {
            return Encoding.UTF8.GetString(data, 3, data.Length - 3);
        }

        return Encoding.UTF8.GetString(data);
    }

    public static Image LoadImage(byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var image = Image.FromStream(stream);
        return new Bitmap(image);
    }

    public static string ToHexPreview(byte[] data, int maxBytes = 4096)
    {
        var length = Math.Min(data.Length, maxBytes);
        var builder = new StringBuilder();

        for (var offset = 0; offset < length; offset += 16)
        {
            builder.Append(offset.ToString("X8"));
            builder.Append("  ");

            var lineLength = Math.Min(16, length - offset);
            for (var i = 0; i < 16; i++)
            {
                if (i < lineLength)
                {
                    builder.Append(data[offset + i].ToString("X2"));
                    builder.Append(' ');
                }
                else
                {
                    builder.Append("   ");
                }
            }

            builder.Append(' ');
            for (var i = 0; i < lineLength; i++)
            {
                var value = data[offset + i];
                builder.Append(value is >= 32 and <= 126 ? (char)value : '.');
            }

            builder.AppendLine();
        }

        if (data.Length > maxBytes)
        {
            builder.AppendLine($"... truncated: {data.Length:N0} bytes total");
        }

        return builder.ToString();
    }

    private static bool LooksLikeKnownBitmap(byte[] data)
    {
        if (data.Length < 8)
        {
            return false;
        }

        var isPng = data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47;
        var isBmp = data[0] == 0x42 && data[1] == 0x4D;
        var isJpeg = data[0] == 0xFF && data[1] == 0xD8;
        var isGif = data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46;

        return isPng || isBmp || isJpeg || isGif;
    }
}
