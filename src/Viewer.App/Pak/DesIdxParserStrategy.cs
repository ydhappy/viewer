using System.Security.Cryptography;

namespace Viewer.App.Pak;

public sealed class DesIdxParserStrategy : IIdxParserStrategy
{
    private const int HeaderSize = 4;
    private const int RecordSize = 28;
    private static readonly byte[] DesKey = { 0x7e, 0x21, 0x40, 0x23, 0x25, 0x5e, 0x24, 0x3c }; // ~!@#%^$<

    public string Name => "des-28";

    public bool IsProbeOnly => false;

    public string Description => "DES ECB encrypted 28-byte IDX parser based on PakViewer LoadIndexDES";

    public IReadOnlyList<IdxRecord> Parse(IdxParseContext context)
    {
        var data = context.IdxBytes;
        if (data.Length < HeaderSize + RecordSize)
        {
            return Array.Empty<IdxRecord>();
        }

        var recordCount = BitConverter.ToInt32(data, 0);
        if (recordCount <= 0 || recordCount > 1_000_000)
        {
            return Array.Empty<IdxRecord>();
        }

        var expectedSize = HeaderSize + recordCount * RecordSize;
        if (expectedSize != data.Length)
        {
            return Array.Empty<IdxRecord>();
        }

        var encryptedLength = data.Length - HeaderSize;
        if (encryptedLength <= 0 || encryptedLength % 8 != 0)
        {
            return Array.Empty<IdxRecord>();
        }

        byte[] entriesData;
        try
        {
            entriesData = DecryptEntries(data.AsSpan(HeaderSize).ToArray());
        }
        catch
        {
            return Array.Empty<IdxRecord>();
        }

        var records = new List<IdxRecord>(recordCount);
        var validCount = 0;
        for (var i = 0; i < recordCount; i++)
        {
            var position = i * RecordSize;
            var offset = BitConverter.ToInt32(entriesData, position);
            var fileName = IdxParserUtilities.ReadNullTerminatedName(entriesData, position + 4, 20);
            var size = BitConverter.ToInt32(entriesData, position + 24);
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

        var ratio = recordCount == 0 ? 0 : (double)validCount / recordCount;
        if (ratio < 0.50)
        {
            return Array.Empty<IdxRecord>();
        }

        return records
            .Where(record => IdxParserUtilities.IsReasonableRecord(record, context.PakSize))
            .ToList();
    }

    private static byte[] DecryptEntries(byte[] encryptedEntries)
    {
        var result = new byte[encryptedEntries.Length];
        using var des = DES.Create();
        des.Key = DesKey;
        des.Mode = CipherMode.ECB;
        des.Padding = PaddingMode.None;

        using var decryptor = des.CreateDecryptor();
        for (var offset = 0; offset < encryptedEntries.Length; offset += 8)
        {
            var decrypted = decryptor.TransformFinalBlock(encryptedEntries, offset, 8);
            Array.Copy(decrypted, 0, result, offset, 8);
        }

        return result;
    }
}
