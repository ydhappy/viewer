namespace Viewer.App.Pak;

public static class PakRecordDiagnostics
{
    public static string BuildRecordSummary(IdxRecord record, string? pakPath = null, long? pakSize = null)
    {
        var lines = new List<string>
        {
            "PAK Record Diagnostics",
            "======================",
            $"FileName       : {record.FileName}",
            $"Index          : {record.Index:N0}",
            $"Format         : {record.Format}",
            $"Offset         : {record.Offset:N0}",
            $"Unpacked Size  : {record.Size:N0}",
            $"Compression    : {record.Compression}",
            $"Compressed Size: {(record.CompressedSize is null ? "-" : record.CompressedSize.Value.ToString("N0"))}",
            $"CanExtract     : {(record.CanExtract ? "YES" : "NO")}" 
        };

        if (!string.IsNullOrWhiteSpace(pakPath))
        {
            lines.Add($"PAK Path       : {pakPath}");
        }

        if (pakSize is not null)
        {
            lines.Add($"PAK Size       : {pakSize.Value:N0}");
        }

        lines.Add(string.Empty);
        lines.Add(BuildCompressionHint(record));
        return string.Join(Environment.NewLine, lines);
    }

    public static string BuildFailureMessage(string reason, IdxRecord record, string? pakPath = null, long? pakSize = null, Exception? inner = null)
    {
        var lines = new List<string>
        {
            reason,
            string.Empty,
            BuildRecordSummary(record, pakPath, pakSize)
        };

        if (inner is not null)
        {
            lines.Add(string.Empty);
            lines.Add("Inner Exception");
            lines.Add("===============");
            lines.Add(inner.GetType().Name + ": " + inner.Message);
        }

        return string.Join(Environment.NewLine, lines);
    }

    public static string BuildCompressionHint(IdxRecord record)
    {
        return record.Compression switch
        {
            0 => "Compression Hint: raw/uncompressed record",
            1 => "Compression Hint: zlib 후보 record",
            2 => "Compression Hint: brotli 후보 record",
            _ => $"Compression Hint: unsupported compression type {record.Compression}"
        };
    }
}
