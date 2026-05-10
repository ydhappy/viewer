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
            BuildStatusMessage(result));
    }

    public static string BuildLogMessage(string idxPath, IdxParseResult result)
    {
        return $"IDX loaded: {idxPath}, strategy={result.StrategyName}, probeOnly={result.IsProbeOnly}, records={result.TotalRecords}, extractable={result.ExtractableRecords}";
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

    private static string BuildStatusMessage(IdxParseResult result)
    {
        if (result.IsProbeOnly)
        {
            return "주의: probe-only/fallback 결과입니다. 현재 단계에서는 실제 추출 가능한 IDX record 해석이 아닐 수 있습니다.";
        }

        return "선택된 strategy가 추출 가능한 record 후보를 생성했습니다.";
    }
}
