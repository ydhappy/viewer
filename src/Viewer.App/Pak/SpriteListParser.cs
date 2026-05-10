using System.Text;

namespace Viewer.App.Pak;

public static class SpriteListParser
{
    public static SpriteListCatalog Parse(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException("list.spr 파일을 찾을 수 없습니다.", filePath);
        }

        var lines = File.ReadAllLines(filePath, DetectEncoding(filePath));
        var entries = new List<SpriteListEntry>();
        var index = 0;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#') || line.StartsWith("//"))
            {
                continue;
            }

            var entry = TryParseLine(index, line);
            if (entry is not null)
            {
                entries.Add(entry);
                index++;
            }
        }

        return new SpriteListCatalog(filePath, entries);
    }

    private static SpriteListEntry? TryParseLine(int index, string line)
    {
        var parts = SplitLine(line);
        if (parts.Length == 0)
        {
            return null;
        }

        var spriteId = TryFindFirstInt(parts) ?? index;
        var name = TryFindName(parts) ?? $"sprite_{spriteId}";
        var group = TryGetPart(parts, 2) ?? string.Empty;
        var action = TryGetPart(parts, 3) ?? string.Empty;
        var frame = TryFindLastInt(parts) ?? 0;

        return new SpriteListEntry(index, spriteId, name, group, action, frame);
    }

    private static string[] SplitLine(string line)
    {
        if (line.Contains(','))
        {
            return line.Split(',')
                .Select(part => part.Trim().Trim('"'))
                .Where(part => part.Length > 0)
                .ToArray();
        }

        if (line.Contains('\t'))
        {
            return line.Split('\t')
                .Select(part => part.Trim().Trim('"'))
                .Where(part => part.Length > 0)
                .ToArray();
        }

        return line.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Select(part => part.Trim().Trim('"'))
            .Where(part => part.Length > 0)
            .ToArray();
    }

    private static int? TryFindFirstInt(IEnumerable<string> parts)
    {
        foreach (var part in parts)
        {
            if (int.TryParse(part, out var value))
            {
                return value;
            }
        }

        return null;
    }

    private static int? TryFindLastInt(IReadOnlyList<string> parts)
    {
        for (var i = parts.Count - 1; i >= 0; i--)
        {
            if (int.TryParse(parts[i], out var value))
            {
                return value;
            }
        }

        return null;
    }

    private static string? TryFindName(IEnumerable<string> parts)
    {
        foreach (var part in parts)
        {
            if (!int.TryParse(part, out _))
            {
                return part;
            }
        }

        return null;
    }

    private static string? TryGetPart(IReadOnlyList<string> parts, int index)
    {
        return index >= 0 && index < parts.Count ? parts[index] : null;
    }

    private static Encoding DetectEncoding(string filePath)
    {
        var bytes = File.ReadAllBytes(filePath);
        if (bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF)
        {
            return Encoding.UTF8;
        }

        return Encoding.UTF8;
    }
}
