using System.Text.RegularExpressions;

namespace Viewer.App.Map;

public static class S32Analyzer
{
    public const int RegionTileWidth = 64;
    public const int RegionTileHeight = 128;
    public const int AttributeWidth = 64;
    public const int AttributeHeight = 64;

    private static readonly Regex CoordinateRegex = new(@"(?<x>-?\d+)[_,-](?<y>-?\d+)", RegexOptions.Compiled);

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
            ExpectedLayer3Cells: AttributeWidth * AttributeHeight,
            Coordinate: TryGuessCoordinate(file.Name),
            LayerCandidateSummary: BuildLayerCandidateSummary(file.Length));
    }

    public static List<S32Info> ScanFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
        {
            throw new DirectoryNotFoundException("지도 폴더를 찾을 수 없습니다: " + folderPath);
        }

        return Directory
            .GetFiles(folderPath, "*.s32", SearchOption.TopDirectoryOnly)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .Select(Analyze)
            .ToList();
    }

    private static S32Coordinate? TryGuessCoordinate(string fileName)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(fileName);
        var match = CoordinateRegex.Match(nameWithoutExt);
        if (!match.Success)
        {
            return null;
        }

        if (!int.TryParse(match.Groups["x"].Value, out var x))
        {
            return null;
        }

        if (!int.TryParse(match.Groups["y"].Value, out var y))
        {
            return null;
        }

        return new S32Coordinate(x, y, "file-name");
    }

    private static string BuildLayerCandidateSummary(long fileSize)
    {
        var layer1Minimum = RegionTileWidth * RegionTileHeight * 2L;
        var layer3Minimum = AttributeWidth * AttributeHeight;
        var knownMinimum = layer1Minimum + layer3Minimum;

        if (fileSize <= 0)
        {
            return "empty or invalid";
        }

        if (fileSize < layer1Minimum)
        {
            return "too small for normal Layer1 candidate";
        }

        if (fileSize < knownMinimum)
        {
            return "Layer1 candidate only";
        }

        return "Layer1/Layer3 candidate, extended layers may exist";
    }
}
