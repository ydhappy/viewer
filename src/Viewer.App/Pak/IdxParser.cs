using System.Text;

namespace Viewer.App.Pak;

public static class IdxParser
{
    private const int ClassicRecordSize = 28;
    private const int ClassicNameOffset = 4;
    private const int ClassicNameLength = 20;
    private const int ClassicSizeOffset = 24;

    public static List<IdxRecord> Parse(string idxPath)
    {
        if (!File.Exists(idxPath))
        {
            throw new FileNotFoundException("IDX 파일을 찾을 수 없습니다.", idxPath);
        }

        var idxBytes = File.ReadAllBytes(idxPath);
        var pakPath = PakExtractor.ResolvePakPath(idxPath);
        var pakSize = File.Exists(pakPath) ? new FileInfo(pakPath).Length : 0;

        var classicRecords = TryParseClassicRecords(idxBytes, pakSize);
        if (classicRecords.Count > 0)
        {
            return classicRecords;
        }

        return CreateFallbackRecords(idxPath, idxBytes);
    }

    private static List<IdxRecord> TryParseClassicRecords(byte[] idxBytes, long pakSize)
    {
        var records = new List<IdxRecord>();

        if (idxBytes.Length < ClassicRecordSize || idxBytes.Length % ClassicRecordSize != 0)
        {
            return records;
        }

        var candidateCount = idxBytes.Length / ClassicRecordSize;
        var validCount = 0;

        for (var i = 0; i < candidateCount; i++)
        {
            var position = i * ClassicRecordSize;
            var offset = BitConverter.ToInt32(idxBytes, position);
            var fileName = ReadNullTerminatedName(idxBytes, position + ClassicNameOffset, ClassicNameLength);
            var size = BitConverter.ToInt32(idxBytes, position + ClassicSizeOffset);
            var canExtract = IsExtractable(offset, size, pakSize);

            if (IsReasonableName(fileName) && offset >= 0 && size >= 0)
            {
                validCount++;
            }

            records.Add(new IdxRecord(
                Index: i + 1,
                FileName: string.IsNullOrWhiteSpace(fileName) ? $"record_{i + 1:D5}.bin" : fileName,
                Size: size,
                Offset: offset,
                CanExtract: canExtract,
                Format: "classic-28"));
        }

        // 너무 적은 후보만 정상으로 보이면 실제 classic IDX가 아닐 가능성이 높다.
        var ratio = candidateCount == 0 ? 0 : (double)validCount / candidateCount;
        return ratio >= 0.50 ? records.Where(r => IsReasonableRecord(r, pakSize)).ToList() : new List<IdxRecord>();
    }

    private static List<IdxRecord> CreateFallbackRecords(string idxPath, byte[] idxBytes)
    {
        var records = new List<IdxRecord>
        {
            new(
                Index: 1,
                FileName: Path.GetFileName(idxPath),
                Size: idxBytes.Length,
                Offset: 0,
                CanExtract: false,
                Format: "fallback-binary")
        };

        TryParseAsciiLikeRecords(idxBytes, records);
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
            if (!IsReasonableName(fileName))
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

    private static string ReadNullTerminatedName(byte[] bytes, int start, int maxLength)
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

    private static bool IsReasonableRecord(IdxRecord record, long pakSize)
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

    private static bool IsExtractable(int offset, int size, long pakSize)
    {
        if (pakSize <= 0 || offset < 0 || size <= 0)
        {
            return false;
        }

        return offset + size <= pakSize;
    }

    private static bool IsReasonableName(string fileName)
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
