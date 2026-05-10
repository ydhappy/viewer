namespace Viewer.App.Pak;

public static class IdxParseResultPresenter
{
    public static string BuildLoadInfo(string idxPath, IdxParseResult result)
    {
        return string.Join(Environment.NewLine,
            "IDX Load Summary",
            "================",
            $"IDX Path   : {idxPath}",
            string.Empty,
            result.ToDisplayText(),
            string.Empty,
            BuildCompressionSummary(result),
            string.Empty,
            BuildStatusMessage(result));
    }

    public static string BuildLogMessage(string idxPath, IdxParseResult result)
    {
        return $"IDX loaded: {idxPath}, strategy={result.StrategyName}, probeOnly={result.IsProbeOnly}, records={result.TotalRecords}, extractable={result.ExtractableRecords}, compressed={result.Records.Count(record => record.Compression != 0)}";
    }

    public static string BuildProbeWarning(IdxParseResult result)
    {
        if (!result.IsProbeOnly)
        {
            return string.Empty;
        }

        return string.Join(Environment.NewLine,
            "이 IDX는 probe-only/fallback 전략으로 인식되었습니다.",
            "아직 실제 추출 가능한 레코드 파서는 아닐 수 있습니다.",
            $"Strategy: {result.StrategyName}",
            $"Message : {result.Message}");
    }

    private static string BuildCompressionSummary(IdxParseResult result)
    {
        var compressed = result.Records.Count(record => record.Compression != 0);
        var raw = result.Records.Count(record => record.Compression == 0);
        var withPackedSize = result.Records.Count(record => record.CompressedSize is > 0);
        var byType = result.Records
            .GroupBy(record => record.Compression)
            .OrderBy(group => group.Key)
            .Select(group => $"- type {group.Key}: {group.Count():N0}");

        return string.Join(Environment.NewLine,
            "Compression Summary",
            "===================",
            $"Raw records       : {raw:N0}",
            $"Compressed records: {compressed:N0}",
            $"Packed size known : {withPackedSize:N0}",
            string.Empty,
            "By Type",
            "-------",
            string.Join(Environment.NewLine, byType));
    }

    private static string BuildStatusMessage(IdxParseResult result)
    {
        if (result.IsProbeOnly)
        {
            return "주의: probe-only/fallback 결과입니다. 현재 단계에서는 실제 추출 가능한 IDX record 해석이 아닐 수 있습니다.";
        }

        if (result.Records.Any(record => record.Compression is 1 or 2))
        {
            return "ExtB 압축 후보가 포함되어 있습니다. compression 1은 zlib, compression 2는 brotli 해제 후보로 처리합니다.";
        }

        return "선택된 strategy가 추출 가능한 record 후보를 생성했습니다.";
    }
}
