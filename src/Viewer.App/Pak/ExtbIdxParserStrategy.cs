using System.Text;

namespace Viewer.App.Pak;

public sealed class ExtbIdxParserStrategy : IIdxParserStrategy
{
    private const int HeaderSize = 0x10;
    private const int EntrySize = 0x80;
    private const int NameOffset = 8;
    private const int NameEndOffset = 120;
    private const int PakOffsetOffset = 120;
    private const int SizeOffset = 124;
    private const int CompressionOffset = 4;

    public string Name => "extb-128";

    public bool IsProbeOnly => false;

    public string Description => "_EXTB$ extended index parser: 16-byte header + 128-byte entries";

    public IReadOnlyList<IdxRecord> Parse(IdxParseContext context)
    {
        var data = context.IdxBytes;
        if (!IsExtbFormat(data))
        {
            return Array.Empty<IdxRecord>();
        }

        if (data.Length < HeaderSize + EntrySize)
        {
            return Array.Empty<IdxRecord>();
        }

        var payloadLength = data.Length - HeaderSize;
        if (payloadLength <= 0 || payloadLength % EntrySize != 0)
        {
            return Array.Empty<IdxRecord>();
        }

        var entries = ReadEntries(data);
        var sortedOffsets = entries
            .Select(entry => entry.PakOffset)
            .Where(offset => offset >= 0 && (context.PakSize <= 0 || offset < context.PakSize))
            .Distinct()
            .OrderBy(offset => offset)
            .ToList();

        var records = new List<IdxRecord>(entries.Count);
        foreach (var entry in entries)
        {
            if (!IdxParserUtilities.IsReasonableName(entry.FileName))
            {
                continue;
            }

            var compressedSize = CalculateCompressedSize(sortedOffsets, entry.PakOffset, context.PakSize);
            var canExtract = entry.FileSize > 0 && entry.PakOffset >= 0 && compressedSize is > 0 &&
                (context.PakSize <= 0 || entry.PakOffset + compressedSize.Value <= context.PakSize);

            records.Add(new IdxRecord(
                Index: records.Count + 1,
                FileName: entry.FileName,
                Size: entry.FileSize,
                Offset: entry.PakOffset,
                CanExtract: canExtract,
                Format: entry.Compression == 0 ? Name : $"{Name}-compressed-{entry.Compression}",
                Compression: entry.Compression,
                CompressedSize: compressedSize));
        }

        return records;
    }

    public static bool IsExtbFormat(byte[] data)
    {
        return data.Length >= 16 &&
               data[0] == (byte)'_' &&
               data[1] == (byte)'E' &&
               data[2] == (byte)'X' &&
               data[3] == (byte)'T' &&
               data[4] == (byte)'B' &&
               data[5] == (byte)'$';
    }

    private static List<ExtbEntry> ReadEntries(byte[] data)
    {
        var payloadLength = data.Length - HeaderSize;
        var entryCount = payloadLength / EntrySize;
        var entries = new List<ExtbEntry>(entryCount);

        for (var i = 0; i < entryCount; i++)
        {
            var entryOffset = HeaderSize + i * EntrySize;
            var compression = BitConverter.ToInt32(data, entryOffset + CompressionOffset);
            var pakOffset = BitConverter.ToInt32(data, entryOffset + PakOffsetOffset);
            var fileSize = BitConverter.ToInt32(data, entryOffset + SizeOffset);
            var fileName = ReadAsciiName(data, entryOffset + NameOffset, entryOffset + NameEndOffset);
            entries.Add(new ExtbEntry(fileName, pakOffset, fileSize, compression));
        }

        return entries;
    }

    private static int? CalculateCompressedSize(IReadOnlyList<int> sortedOffsets, int offset, long pakSize)
    {
        if (offset < 0)
        {
            return null;
        }

        var index = sortedOffsets.IndexOf(offset);
        if (index < 0)
        {
            return null;
        }

        if (index + 1 < sortedOffsets.Count)
        {
            return Math.Max(0, sortedOffsets[index + 1] - offset);
        }

        if (pakSize > offset)
        {
            return (int)(pakSize - offset);
        }

        return null;
    }

    private static string ReadAsciiName(byte[] data, int start, int endExclusive)
    {
        var end = start;
        while (end < endExclusive && end < data.Length && data[end] != 0 && data[end] >= 32 && data[end] <= 126)
        {
            end++;
        }

        if (end <= start)
        {
            return string.Empty;
        }

        return Encoding.ASCII.GetString(data, start, end - start).Trim();
    }

    private sealed record ExtbEntry(string FileName, int PakOffset, int FileSize, int Compression);
}
