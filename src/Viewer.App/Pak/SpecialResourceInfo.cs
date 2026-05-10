using System.Text;

namespace Viewer.App.Pak;

public enum SpecialResourceKind
{
    None,
    Sprite,
    ImageRaw,
    Tile,
    TileTable
}

public sealed record SpecialResourceInfo(
    SpecialResourceKind Kind,
    string Extension,
    int Size,
    string Summary,
    string HexHeader
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "Special Resource",
            "================",
            $"Kind      : {Kind}",
            $"Extension : {Extension}",
            $"Size      : {Size:N0} bytes",
            $"Summary   : {Summary}",
            string.Empty,
            "Header",
            "------",
            HexHeader,
            string.Empty,
            "※ 5차에서는 전용 포맷 감지/정보 표시까지만 처리합니다.",
            "※ 실제 SPR/IMG/TIL 렌더링은 다음 단계에서 변환기 단위로 연결합니다.");
    }
}

public static class SpecialResourceAnalyzer
{
    private static readonly HashSet<string> SpriteExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".spr"
    };

    private static readonly HashSet<string> RawImageExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".img"
    };

    private static readonly HashSet<string> TileExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".til"
    };

    private static readonly HashSet<string> TileTableExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".tbt"
    };

    public static bool IsSpecialResource(string fileName)
    {
        return DetectKind(fileName) != SpecialResourceKind.None;
    }

    public static SpecialResourceInfo Analyze(string fileName, byte[] data)
    {
        var extension = Path.GetExtension(fileName);
        var kind = DetectKind(fileName);
        var summary = kind switch
        {
            SpecialResourceKind.Sprite => "Lineage sprite resource candidate (.spr)",
            SpecialResourceKind.ImageRaw => "Lineage raw image resource candidate (.img)",
            SpecialResourceKind.Tile => "Lineage tile resource candidate (.til)",
            SpecialResourceKind.TileTable => "Lineage tile table resource candidate (.tbt)",
            _ => "Not a special Lineage resource"
        };

        return new SpecialResourceInfo(
            Kind: kind,
            Extension: extension,
            Size: data.Length,
            Summary: summary,
            HexHeader: BuildHeaderPreview(data));
    }

    private static SpecialResourceKind DetectKind(string fileName)
    {
        var extension = Path.GetExtension(fileName);

        if (SpriteExtensions.Contains(extension))
        {
            return SpecialResourceKind.Sprite;
        }

        if (RawImageExtensions.Contains(extension))
        {
            return SpecialResourceKind.ImageRaw;
        }

        if (TileExtensions.Contains(extension))
        {
            return SpecialResourceKind.Tile;
        }

        if (TileTableExtensions.Contains(extension))
        {
            return SpecialResourceKind.TileTable;
        }

        return SpecialResourceKind.None;
    }

    private static string BuildHeaderPreview(byte[] data)
    {
        var length = Math.Min(data.Length, 128);
        if (length == 0)
        {
            return "<empty>";
        }

        var builder = new StringBuilder();
        for (var offset = 0; offset < length; offset += 16)
        {
            builder.Append(offset.ToString("X8"));
            builder.Append("  ");

            var lineLength = Math.Min(16, length - offset);
            for (var i = 0; i < lineLength; i++)
            {
                builder.Append(data[offset + i].ToString("X2"));
                builder.Append(' ');
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }
}
