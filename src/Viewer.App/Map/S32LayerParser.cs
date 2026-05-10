namespace Viewer.App.Map;

public static class S32LayerParser
{
    private const int Layer1TileCount = S32Analyzer.RegionTileWidth * S32Analyzer.RegionTileHeight;
    private const int Layer1Bytes = Layer1TileCount * sizeof(ushort);

    public static S32LayerSample ParseLayer1Sample(string s32Path)
    {
        if (!File.Exists(s32Path))
        {
            throw new FileNotFoundException("S32 파일을 찾을 수 없습니다.", s32Path);
        }

        var fileInfo = new FileInfo(s32Path);
        if (fileInfo.Length < Layer1Bytes)
        {
            return new S32LayerSample(s32Path, Array.Empty<ushort>(), 0);
        }

        var tileIds = new ushort[Layer1TileCount];
        using var stream = new FileStream(s32Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        using var reader = new BinaryReader(stream);

        for (var i = 0; i < tileIds.Length; i++)
        {
            tileIds[i] = reader.ReadUInt16();
        }

        return new S32LayerSample(s32Path, tileIds, Layer1Bytes);
    }
}
