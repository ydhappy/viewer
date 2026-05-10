using System.Text;

namespace Viewer.App.Pak;

public sealed record IdxWriteResult(
    bool Success,
    string Message,
    string OutputIdxPath,
    int RecordCount,
    long BytesWritten
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "IDX Write Result",
            "================",
            $"Success     : {Success}",
            $"Message     : {Message}",
            $"Output IDX  : {OutputIdxPath}",
            $"Record count: {RecordCount:N0}",
            $"Bytes       : {BytesWritten:N0}");
    }
}

public static class IdxWriter
{
    private const int ClassicRecordSize = 28;
    private const int ClassicNameLength = 20;

    public static string GetDefaultRebuiltIdxPath(string sourceIdxPath)
    {
        var directory = Path.GetDirectoryName(sourceIdxPath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(sourceIdxPath);
        var extension = Path.GetExtension(sourceIdxPath);
        return Path.Combine(directory, fileNameWithoutExtension + ".rebuilt" + extension);
    }

    public static IdxWriteResult WriteClassic28(string outputIdxPath, IEnumerable<IdxRecord> records, bool overwrite = false)
    {
        var recordList = records.ToList();
        if (File.Exists(outputIdxPath) && !overwrite)
        {
            return new IdxWriteResult(false, "Output IDX already exists.", outputIdxPath, recordList.Count, new FileInfo(outputIdxPath).Length);
        }

        var validation = ValidateClassic28Records(recordList);
        if (!validation.Success)
        {
            return new IdxWriteResult(false, validation.Message, outputIdxPath, recordList.Count, 0);
        }

        try
        {
            using var stream = new FileStream(outputIdxPath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None);
            using var writer = new BinaryWriter(stream, Encoding.Default, leaveOpen: true);

            foreach (var record in recordList)
            {
                WriteClassic28Record(writer, record);
            }

            writer.Flush();
            stream.Flush(flushToDisk: true);

            return new IdxWriteResult(true, "Classic28 IDX write completed.", outputIdxPath, recordList.Count, stream.Length);
        }
        catch (Exception ex)
        {
            var bytesWritten = File.Exists(outputIdxPath) ? new FileInfo(outputIdxPath).Length : 0;
            return new IdxWriteResult(false, "Classic28 IDX write failed: " + ex.Message, outputIdxPath, recordList.Count, bytesWritten);
        }
    }

    public static IdxWriteResult WriteClassic28ForRebuild(string sourceIdxPath, PakRebuildResult rebuildResult, bool overwrite = false)
    {
        var outputIdxPath = GetDefaultRebuiltIdxPath(sourceIdxPath);
        var rebuiltRecords = rebuildResult.Records
            .OrderBy(record => record.NewOffset)
            .ThenBy(record => record.Source.Index)
            .Select((record, index) => record.ToIdxRecord() with { Index = index + 1, Format = "classic-28" })
            .ToList();

        return WriteClassic28(outputIdxPath, rebuiltRecords, overwrite);
    }

    private static void WriteClassic28Record(BinaryWriter writer, IdxRecord record)
    {
        writer.Write(record.Offset);
        var nameBytes = EncodeClassicName(record.FileName);
        writer.Write(nameBytes);
        writer.Write(record.Size);
    }

    private static byte[] EncodeClassicName(string fileName)
    {
        var encoded = Encoding.Default.GetBytes(fileName);
        if (encoded.Length > ClassicNameLength)
        {
            throw new InvalidOperationException($"File name is too long for classic IDX record: {fileName}");
        }

        var buffer = new byte[ClassicNameLength];
        Array.Copy(encoded, buffer, encoded.Length);
        return buffer;
    }

    private static IdxWriteResult ValidateClassic28Records(IReadOnlyList<IdxRecord> records)
    {
        foreach (var record in records)
        {
            if (record.Offset < 0)
            {
                return new IdxWriteResult(false, $"Invalid offset: {record.FileName}", string.Empty, records.Count, 0);
            }

            if (record.Size < 0)
            {
                return new IdxWriteResult(false, $"Invalid size: {record.FileName}", string.Empty, records.Count, 0);
            }

            var encodedName = Encoding.Default.GetBytes(record.FileName);
            if (encodedName.Length > ClassicNameLength)
            {
                return new IdxWriteResult(false, $"File name exceeds {ClassicNameLength} bytes: {record.FileName}", string.Empty, records.Count, 0);
            }
        }

        return new IdxWriteResult(true, "Classic28 validation passed.", string.Empty, records.Count, records.Count * ClassicRecordSize);
    }
}
