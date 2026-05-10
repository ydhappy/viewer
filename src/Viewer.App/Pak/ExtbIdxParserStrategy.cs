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

    public string Description => "_EXTB$ extended index parser skeleton: 16-byte header + 128-byte entries";

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

        var entryCount = payloadLength / EntrySize;
        var records = new List<IdxRecord>(entryCount);

        for (var i = 0; i < entryCount; i++)
        {
            var entryOffset = HeaderSize + i * EntrySize;
            var compression = BitConverter.ToInt32(data, entryOffset + CompressionOffset);
            var pakOffset = BitConverter.ToInt32(data, entryOffset + PakOffsetOffset);
            var fileSize = BitConverter.ToInt32(data, entryOffset + SizeOffset);
            var fileName = ReadAsciiName(data, entryOffset + NameOffset, entryOffset + NameEndOffset);

            if (!IdxParserUtilities.IsReasonableName(fileName))
            {
                continue;
            }

            var canExtract = compression == 0 && IdxParserUtilities.IsExtractable(pakOffset, fileSize, context.PakSize);
            records.Add(new IdxRecord(
                Index: records.Count + 1,
                FileName: fileName,
                Size: fileSize,
                Offset: pakOffset,
                CanExtract: canExtract,
                Format: compression == 0 ? Name : $"{Name}-compressed-{compression}"));
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
}
