using System.Text;

namespace Viewer.App.Pak;

public sealed record IdxParseContext(
    string IdxPath,
    byte[] IdxBytes,
    string PakPath,
    long PakSize
);

public sealed record IdxParseResult(
    IReadOnlyList<IdxRecord> Records,
    string StrategyName,
    string StrategyListText,
    bool IsProbeOnly,
    string Message
)
{
    public int TotalRecords => Records.Count;

    public int ExtractableRecords => Records.Count(record => record.CanExtract);

    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "IDX Parse Result",
            "================",
            $"Strategy   : {StrategyName}",
            $"Records    : {TotalRecords:N0}",
            $"Extractable: {ExtractableRecords:N0}",
            $"Probe Only : {(IsProbeOnly ? "YES" : "NO")}",
            $"Message    : {Message}",
            string.Empty,
            StrategyListText);
    }
}

public interface IIdxParserStrategy
{
    string Name { get; }

    bool IsProbeOnly { get; }

    string Description { get; }

    IReadOnlyList<IdxRecord> Parse(IdxParseContext context);
}

public sealed class Classic28IdxParserStrategy : IIdxParserStrategy
{
    private const int RecordSize = 28;
    private const int NameOffset = 4;
    private const int NameLength = 20;
    private const int SizeOffset = 24;

    public string Name => "classic-28";

    public bool IsProbeOnly => false;

    public string Description => "offset(4) + filename(20) + size(4) 후보 레코드 파서";

    public IReadOnlyList<IdxRecord> Parse(IdxParseContext context)
    {
        var records = new List<IdxRecord>();
        var idxBytes = context.IdxBytes;

        if (idxBytes.Length < RecordSize || idxBytes.Length % RecordSize != 0)
        {
            return records;
        }

        var candidateCount = idxBytes.Length / RecordSize;
        var validCount = 0;

        for (var i = 0; i < candidateCount; i++)
        {
            var position = i * RecordSize;
            var offset = BitConverter.ToInt32(idxBytes, position);
            var fileName = IdxParserUtilities.ReadNullTerminatedName(idxBytes, position + NameOffset, NameLength);
            var size = BitConverter.ToInt32(idxBytes, position + SizeOffset);
            var canExtract = IdxParserUtilities.IsExtractable(offset, size, context.PakSize);

            if (IdxParserUtilities.IsReasonableName(fileName) && offset >= 0 && size >= 0)
            {
                validCount++;
            }

            records.Add(new IdxRecord(
                Index: i + 1,
                FileName: string.IsNullOrWhiteSpace(fileName) ? $"record_{i + 1:D5}.bin" : fileName,
                Size: size,
                Offset: offset,
                CanExtract: canExtract,
                Format: Name));
        }

        var ratio = candidateCount == 0 ? 0 : (double)validCount / candidateCount;
        if (ratio < 0.50)
        {
            return Array.Empty<IdxRecord>();
        }

        return records
            .Where(record => IdxParserUtilities.IsReasonableRecord(record, context.PakSize))
            .ToList();
    }
}

public sealed class ExtbHeaderProbeIdxParserStrategy : IIdxParserStrategy
{
    public string Name => "probe-extb-header";

    public bool IsProbeOnly => true;

    public string Description => "_EXTB$ marker probe. 실제 확장 레코드 파서는 아님";

    public IReadOnlyList<IdxRecord> Parse(IdxParseContext context)
    {
        if (!ContainsAsciiMarker(context.IdxBytes, "_EXTB$"))
        {
            return Array.Empty<IdxRecord>();
        }

        return new[]
        {
            new IdxRecord(
                Index: 1,
                FileName: Path.GetFileName(context.IdxPath),
                Size: context.IdxBytes.Length,
                Offset: 0,
                CanExtract: false,
                Format: Name)
        };
    }

    private static bool ContainsAsciiMarker(byte[] bytes, string marker)
    {
        var markerBytes = Encoding.ASCII.GetBytes(marker);
        if (bytes.Length < markerBytes.Length)
        {
            return false;
        }

        var max = Math.Min(bytes.Length - markerBytes.Length, 4096);
        for (var i = 0; i <= max; i++)
        {
            var matched = true;
            for (var j = 0; j < markerBytes.Length; j++)
            {
                if (bytes[i + j] != markerBytes[j])
                {
                    matched = false;
                    break;
                }
            }

            if (matched)
            {
                return true;
            }
        }

        return false;
    }
}

public sealed class FallbackIdxParserStrategy : IIdxParserStrategy
{
    public string Name => "fallback";

    public bool IsProbeOnly => true;

    public string Description => "binary/text fallback 표시용. 추출 가능한 레코드 파서 아님";

    public IReadOnlyList<IdxRecord> Parse(IdxParseContext context)
    {
        var records = new List<IdxRecord>
        {
            new(
                Index: 1,
                FileName: Path.GetFileName(context.IdxPath),
                Size: context.IdxBytes.Length,
                Offset: 0,
                CanExtract: false,
                Format: "fallback-binary")
        };

        TryParseAsciiLikeRecords(context.IdxBytes, records);
        return records;
    }

    private static void TryParseAsciiLikeRecords(byte[] bytes, List<IdxRecord> records)
    {
        var text = Encoding.UTF8.GetString(bytes);
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var nextIndex = records.Count + 1;

        foreach (var line in lines.Take(1000))
        {
            if (!line.Contains('.'))
            {
                continue;
            }

            var fileName = line.Trim();
            if (!IdxParserUtilities.IsReasonableName(fileName))
            {
                continue;
            }

            records.Add(new IdxRecord(
                Index: nextIndex++,
                FileName: fileName,
                Size: 0,
                Offset: 0,
                CanExtract: false,
                Format: "fallback-text"));
        }
    }
}

public sealed class IdxParserStrategyRegistry
{
    private readonly List<IIdxParserStrategy> _strategies;

    public IdxParserStrategyRegistry(IEnumerable<IIdxParserStrategy> strategies)
    {
        _strategies = strategies.ToList();
    }

    public static IdxParserStrategyRegistry CreateDefault()
    {
        return new IdxParserStrategyRegistry(new IIdxParserStrategy[]
        {
            new ExtbIdxParserStrategy(),
            new Classic28IdxParserStrategy(),
            new ExtbHeaderProbeIdxParserStrategy(),
            new FallbackIdxParserStrategy()
        });
    }

    public IReadOnlyList<IIdxParserStrategy> Strategies => _strategies;

    public IReadOnlyList<IdxRecord> Parse(IdxParseContext context)
    {
        return ParseDetailed(context).Records;
    }

    public IdxParseResult ParseDetailed(IdxParseContext context)
    {
        foreach (var strategy in _strategies)
        {
            var records = strategy.Parse(context);
            if (records.Count > 0)
            {
                return new IdxParseResult(
                    records,
                    strategy.Name,
                    ToDisplayText(),
                    strategy.IsProbeOnly,
                    strategy.Description);
            }
        }

        return new IdxParseResult(
            Array.Empty<IdxRecord>(),
            "none",
            ToDisplayText(),
            true,
            "No IDX parser strategy produced records.");
    }

    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "Registered IDX Parser Strategies",
            "===============================",
            _strategies.Select(strategy => $"- {strategy.Name}: {strategy.Description}"));
    }
}

internal static class IdxParserUtilities
{
    public static string ReadNullTerminatedName(byte[] bytes, int start, int maxLength)
    {
        var length = 0;
        while (length < maxLength && start + length < bytes.Length && bytes[start + length] != 0)
        {
            length++;
        }

        if (length == 0)
        {
            return string.Empty;
        }

        return Encoding.Default.GetString(bytes, start, length).Trim();
    }

    public static bool IsReasonableRecord(IdxRecord record, long pakSize)
    {
        if (!IsReasonableName(record.FileName))
        {
            return false;
        }

        if (record.Offset < 0 || record.Size < 0)
        {
            return false;
        }

        if (pakSize > 0 && record.Offset + record.Size > pakSize)
        {
            return false;
        }

        return true;
    }

    public static bool IsExtractable(int offset, int size, long pakSize)
    {
        if (pakSize <= 0 || offset < 0 || size <= 0)
        {
            return false;
        }

        return offset + size <= pakSize;
    }

    public static bool IsReasonableName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 260)
        {
            return false;
        }

        if (fileName.Any(char.IsControl))
        {
            return false;
        }

        return fileName.Any(char.IsLetterOrDigit) && fileName.Contains('.');
    }
}
