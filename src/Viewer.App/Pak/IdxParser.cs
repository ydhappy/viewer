namespace Viewer.App.Pak;

public static class IdxParser
{
    private static readonly IdxParserStrategyRegistry Registry = IdxParserStrategyRegistry.CreateDefault();

    public static List<IdxRecord> Parse(string idxPath)
    {
        return ParseDetailed(idxPath).Records.ToList();
    }

    public static IdxParseResult ParseDetailed(string idxPath)
    {
        if (!File.Exists(idxPath))
        {
            throw new FileNotFoundException("IDX 파일을 찾을 수 없습니다.", idxPath);
        }

        var idxBytes = File.ReadAllBytes(idxPath);
        var pakPath = PakExtractor.ResolvePakPath(idxPath);
        var pakSize = File.Exists(pakPath) ? new FileInfo(pakPath).Length : 0;
        var context = new IdxParseContext(idxPath, idxBytes, pakPath, pakSize);

        return Registry.ParseDetailed(context);
    }

    public static string GetStrategyListText()
    {
        return Registry.ToDisplayText();
    }
}
