namespace Viewer.App.Map;

public static class S32Analyzer
{
    public const int RegionTileWidth = 64;
    public const int RegionTileHeight = 128;
    public const int AttributeWidth = 64;
    public const int AttributeHeight = 64;

    public static S32Info Analyze(string s32Path)
    {
        if (!File.Exists(s32Path))
        {
            throw new FileNotFoundException("S32 파일을 찾을 수 없습니다.", s32Path);
        }

        var file = new FileInfo(s32Path);
        return new S32Info(
            FilePath: s32Path,
            FileSize: file.Length,
            ExpectedLayer1Tiles: RegionTileWidth * RegionTileHeight,
            ExpectedLayer3Cells: AttributeWidth * AttributeHeight);
    }
}
