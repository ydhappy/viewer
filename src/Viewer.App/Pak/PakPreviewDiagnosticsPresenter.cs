namespace Viewer.App.Pak;

public static class PakPreviewDiagnosticsPresenter
{
    public static string BuildInitialInfo(IdxRecord record, string? currentIdxPath)
    {
        var pakPath = ResolvePakPathOrNull(currentIdxPath);
        var pakSize = ResolvePakSizeOrNull(pakPath);
        return IdxLoadUiBinder.BuildRecordInfo(record, pakPath, pakSize);
    }

    public static string BuildPreviewSuccessInfo(PreviewKind kind, int readBytes)
    {
        return string.Join(Environment.NewLine,
            "Preview Result",
            "==============",
            $"Preview  : {kind}",
            $"ReadBytes: {readBytes:N0}");
    }

    public static string BuildPreviewFailureInfo(IdxRecord record, Exception exception, string? currentIdxPath)
    {
        var pakPath = ResolvePakPathOrNull(currentIdxPath);
        var pakSize = ResolvePakSizeOrNull(pakPath);

        return string.Join(Environment.NewLine,
            "Preview error",
            "=============",
            exception.Message,
            string.Empty,
            PakRecordDiagnostics.BuildRecordSummary(record, pakPath, pakSize));
    }

    public static string BuildPreviewFailureLog(IdxRecord record, Exception exception)
    {
        return $"Preview failed: {record.FileName} - compression={record.Compression}, packed={record.CompressedSize?.ToString() ?? "-"}, error={exception.Message}";
    }

    public static string? ResolvePakPathOrNull(string? currentIdxPath)
    {
        if (string.IsNullOrWhiteSpace(currentIdxPath))
        {
            return null;
        }

        return PakExtractor.ResolvePakPath(currentIdxPath);
    }

    public static long? ResolvePakSizeOrNull(string? pakPath)
    {
        if (string.IsNullOrWhiteSpace(pakPath) || !File.Exists(pakPath))
        {
            return null;
        }

        return new FileInfo(pakPath).Length;
    }
}
