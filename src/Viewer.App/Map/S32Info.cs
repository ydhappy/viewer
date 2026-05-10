namespace Viewer.App.Map;

public sealed record S32Info(
    string FilePath,
    long FileSize,
    int ExpectedLayer1Tiles,
    int ExpectedLayer3Cells,
    S32Coordinate? Coordinate,
    string LayerCandidateSummary
)
{
    public string FileName => Path.GetFileName(FilePath);

    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "S32 Map Info",
            "============",
            $"File      : {FilePath}",
            $"Name      : {FileName}",
            $"Size      : {FileSize:N0} bytes",
            $"Coordinate: {(Coordinate is null ? "unknown" : Coordinate)}",
            $"Layer1    : {ExpectedLayer1Tiles:N0} floor tiles expected",
            $"Layer3    : {ExpectedLayer3Cells:N0} attribute cells expected",
            $"Candidate : {LayerCandidateSummary}",
            string.Empty,
            "※ 6차 분석기는 파일명 좌표와 크기 기반 후보만 표시합니다.",
            "※ 실제 레이어 렌더링은 Tile.idx 연동 단계에서 처리합니다.");
    }
}
