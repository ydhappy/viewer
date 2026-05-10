namespace Viewer.App.Pak;

public sealed record PakRebuildRecord(
    IdxRecord Source,
    int NewOffset,
    int NewSize
)
{
    public IdxRecord ToIdxRecord()
    {
        return Source with { Offset = NewOffset, Size = NewSize };
    }
}

public sealed record PakRebuildResult(
    bool Success,
    string Message,
    string SourcePakPath,
    string OutputPakPath,
    IReadOnlyList<PakRebuildRecord> Records,
    IReadOnlyList<IdxRecord> DeletedRecords,
    long SourceSize,
    long OutputSize
)
{
    public int KeptCount => Records.Count;
    public int DeletedCount => DeletedRecords.Count;

    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "PAK Rebuild Result",
            "==================",
            $"Success   : {Success}",
            $"Message   : {Message}",
            $"Source    : {SourcePakPath}",
            $"Output    : {OutputPakPath}",
            $"SourceSize: {SourceSize:N0}",
            $"OutputSize: {OutputSize:N0}",
            $"Kept      : {KeptCount:N0}",
            $"Deleted   : {DeletedCount:N0}");
    }
}

public static class PakRebuilder
{
    public static string GetDefaultOutputPath(string pakPath)
    {
        var directory = Path.GetDirectoryName(pakPath) ?? string.Empty;
        var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(pakPath);
        var extension = Path.GetExtension(pakPath);
        return Path.Combine(directory, fileNameWithoutExtension + ".rebuilt" + extension);
    }

    public static PakRebuildResult RebuildWithoutRecords(
        string sourcePakPath,
        IEnumerable<IdxRecord> sourceRecords,
        IEnumerable<IdxRecord> recordsToDelete,
        string? outputPakPath = null,
        bool overwrite = false)
    {
        outputPakPath ??= GetDefaultOutputPath(sourcePakPath);
        var sourceList = sourceRecords.ToList();
        var deleteKeys = recordsToDelete.Select(GetRecordKey).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var deleted = sourceList.Where(record => deleteKeys.Contains(GetRecordKey(record))).ToList();
        var kept = sourceList.Where(record => !deleteKeys.Contains(GetRecordKey(record))).ToList();

        if (!File.Exists(sourcePakPath))
        {
            return Fail("Source PAK file does not exist.", sourcePakPath, outputPakPath, kept, deleted, 0, 0);
        }

        if (File.Exists(outputPakPath) && !overwrite)
        {
            return Fail("Output PAK already exists.", sourcePakPath, outputPakPath, kept, deleted, new FileInfo(sourcePakPath).Length, new FileInfo(outputPakPath).Length);
        }

        try
        {
            var sourceSize = new FileInfo(sourcePakPath).Length;
            var rebuiltRecords = new List<PakRebuildRecord>(kept.Count);

            using var source = new FileStream(sourcePakPath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using var output = new FileStream(outputPakPath, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None);

            foreach (var record in kept.OrderBy(record => record.Offset).ThenBy(record => record.Index))
            {
                if (!CanCopyRecord(record, sourceSize, out var reason))
                {
                    return Fail($"Cannot rebuild because record '{record.FileName}' is invalid: {reason}", sourcePakPath, outputPakPath, rebuiltRecords, deleted, sourceSize, output.Length);
                }

                var newOffset = checked((int)output.Position);
                source.Seek(record.Offset, SeekOrigin.Begin);
                CopyExactly(source, output, record.Size);
                rebuiltRecords.Add(new PakRebuildRecord(record, newOffset, record.Size));
            }

            output.Flush(flushToDisk: true);
            var outputSize = output.Length;

            return new PakRebuildResult(
                true,
                "PAK rebuild completed. IDX rewrite is not included in this step.",
                sourcePakPath,
                outputPakPath,
                rebuiltRecords,
                deleted,
                sourceSize,
                outputSize);
        }
        catch (Exception ex)
        {
            var sourceSize = File.Exists(sourcePakPath) ? new FileInfo(sourcePakPath).Length : 0;
            var outputSize = File.Exists(outputPakPath) ? new FileInfo(outputPakPath).Length : 0;
            return Fail("PAK rebuild failed: " + ex.Message, sourcePakPath, outputPakPath, kept, deleted, sourceSize, outputSize);
        }
    }

    private static bool CanCopyRecord(IdxRecord record, long sourceSize, out string reason)
    {
        if (!record.CanExtract)
        {
            reason = "not extractable";
            return false;
        }

        if (record.Offset < 0 || record.Size < 0)
        {
            reason = "negative offset or size";
            return false;
        }

        if (record.Offset + record.Size > sourceSize)
        {
            reason = "range exceeds source size";
            return false;
        }

        reason = string.Empty;
        return true;
    }

    private static void CopyExactly(Stream source, Stream output, int byteCount)
    {
        var buffer = new byte[81920];
        var remaining = byteCount;
        while (remaining > 0)
        {
            var read = source.Read(buffer, 0, Math.Min(buffer.Length, remaining));
            if (read <= 0)
            {
                throw new EndOfStreamException("Unexpected end of PAK stream during rebuild.");
            }

            output.Write(buffer, 0, read);
            remaining -= read;
        }
    }

    private static string GetRecordKey(IdxRecord record)
    {
        return string.Join("|", record.Index, record.FileName, record.Offset, record.Size);
    }

    private static PakRebuildResult Fail(string message, string sourcePakPath, string outputPakPath, IReadOnlyList<IdxRecord> keptRecords, IReadOnlyList<IdxRecord> deletedRecords, long sourceSize, long outputSize)
    {
        return new PakRebuildResult(
            false,
            message,
            sourcePakPath,
            outputPakPath,
            keptRecords.Select(record => new PakRebuildRecord(record, record.Offset, record.Size)).ToList(),
            deletedRecords,
            sourceSize,
            outputSize);
    }
}
