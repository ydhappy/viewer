namespace Viewer.App.Pak;

public static class IdxParser
{
    private static readonly IdxParserStrategyRegistry Registry = IdxParserStrategyRegistry.CreateDefault();

    public static List<IdxRecord> Parse(string idxPath)
    {
        if (!File.Exists(idxPath))
        {
            throw new FileNotFoundException("IDX 파일을 찾을 수 없습니다.", idxPath);
        }

        var idxBytes = File.ReadAllBytes(idxPath);
        var pakPath = PakExtractor.ResolvePakPath(idxPath);
        var pakSize = File.Exists(pakPath) ? new FileInfo(pakPath).Length : 0;
        var context = new IdxParseContext(idxPath, idxBytes, pakPath, pakSize);

        return Registry.Parse(context).ToList();
    }

    public static string GetStrategyListText()
    {
        return Registry.ToDisplayText();
    }
}
