namespace Viewer.App.Map;

public interface ITileImageCache
{
    TileConversionResult GetTileImage(int tileId, TileResourceSet tileResourceSet);
}

public sealed class NullTileImageCache : ITileImageCache
{
    public TileConversionResult GetTileImage(int tileId, TileResourceSet tileResourceSet)
    {
        var record = tileResourceSet.FindByTileId(tileId);
        var candidate = record is null ? TileConversionCandidate.Unknown : TileResourceClassifier.Classify(record);
        return new TileConversionResult(
            tileId,
            record,
            candidate,
            false,
            null,
            "NullCache",
            "Tile 이미지 캐시가 연결되지 않았습니다.");
    }
}

public sealed class TileRecordLookup
{
    public TileRecordLookup(int tileId, TileConversionResult conversionResult)
    {
        TileId = tileId;
        ConversionResult = conversionResult;
    }

    public int TileId { get; }

    public TileConversionResult ConversionResult { get; }

    public string ToDisplayText()
    {
        return ConversionResult.ToDisplayText();
    }
}
