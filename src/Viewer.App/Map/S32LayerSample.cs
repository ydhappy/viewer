namespace Viewer.App.Map;

public sealed class S32LayerSample
{
    public const int Width = S32Analyzer.RegionTileWidth;
    public const int Height = S32Analyzer.RegionTileHeight;

    public S32LayerSample(string filePath, ushort[] layer1TileIds, long bytesRead)
    {
        FilePath = filePath;
        Layer1TileIds = layer1TileIds;
        BytesRead = bytesRead;
    }

    public string FilePath { get; }

    public ushort[] Layer1TileIds { get; }

    public long BytesRead { get; }

    public int Count => Layer1TileIds.Length;

    public bool HasLayer1 => Layer1TileIds.Length == Width * Height;

    public ushort GetTileId(int x, int y)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
        {
            return 0;
        }

        var index = y * Width + x;
        return index >= 0 && index < Layer1TileIds.Length ? Layer1TileIds[index] : (ushort)0;
    }
}
