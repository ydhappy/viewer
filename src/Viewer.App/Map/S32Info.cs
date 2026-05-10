namespace Viewer.App.Map;

public sealed record S32Info(
    string FilePath,
    long FileSize,
    int ExpectedLayer1Tiles,
    int ExpectedLayer3Cells
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "S32 Map Info",
            "============",
            $"File      : {FilePath}",
            $"Size      : {FileSize:N0} bytes",
            $"Layer1    : {ExpectedLayer1Tiles:N0} floor tiles expected",
            $"Layer3    : {ExpectedLayer3Cells:N0} attribute cells expected",
            string.Empty,
            "※ 1차 분석기는 파일 기본 정보만 표시합니다.",
            "※ 다음 단계에서 L1MapViewer의 S32 레이어 파서를 흡수합니다.");
    }
}
