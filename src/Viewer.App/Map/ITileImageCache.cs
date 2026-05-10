using Viewer.App.Pak;

namespace Viewer.App.Map;

public interface ITileImageCache
{
    bool TryGetTileImage(int tileId, TileResourceSet tileResourceSet, out Image? image);
}

public sealed class NullTileImageCache : ITileImageCache
{
    public bool TryGetTileImage(int tileId, TileResourceSet tileResourceSet, out Image? image)
    {
        image = null;
        return false;
    }
}

public sealed class TileRecordLookup
{
    public TileRecordLookup(int tileId, IdxRecord? record, bool hasImage)
    {
        TileId = tileId;
        Record = record;
        HasImage = hasImage;
    }

    public int TileId { get; }

    public IdxRecord? Record { get; }

    public bool HasImage { get; }

    public string ToDisplayText()
    {
        if (Record is null)
        {
            return $"Tile ID {TileId}에 해당하는 레코드를 찾지 못했습니다.";
        }

        return string.Join(Environment.NewLine,
            "Tile Record Lookup",
            "==================",
            $"Tile ID     : {TileId}",
            $"Record Index: {Record.Index}",
            $"FileName    : {Record.FileName}",
            $"Offset      : {Record.Offset:N0}",
            $"Size        : {Record.Size:N0}",
            $"CanExtract  : {(Record.CanExtract ? "YES" : "NO")}",
            $"Format      : {Record.Format}",
            $"Image Cache : {(HasImage ? "HIT" : "MISS / not implemented")}");
    }
}
