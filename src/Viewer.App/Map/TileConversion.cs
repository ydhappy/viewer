using Viewer.App.Pak;

namespace Viewer.App.Map;

public enum TileResourceKind
{
    Unknown,
    DirectImage,
    Sprite,
    RawImage,
    Tile,
    TileTable,
    Text,
    Binary
}

public sealed record TileConversionCandidate(
    TileResourceKind Kind,
    string Extension,
    bool CanAttemptDirectImage,
    string Description
)
{
    public static TileConversionCandidate Unknown { get; } = new(
        TileResourceKind.Unknown,
        string.Empty,
        false,
        "Unknown tile resource type");
}

public sealed record TileConversionResult(
    int TileId,
    IdxRecord? Record,
    TileConversionCandidate Candidate,
    bool Success,
    Image? Image,
    string ConverterName,
    string Message
)
{
    public static TileConversionResult NotFound(int tileId, string message)
    {
        return new TileConversionResult(tileId, null, TileConversionCandidate.Unknown, false, null, "none", message);
    }

    public string ToDisplayText()
    {
        if (Record is null)
        {
            return string.Join(Environment.NewLine,
                "Tile Conversion",
                "===============",
                $"Tile ID   : {TileId}",
                "Record    : not found",
                $"Converter : {ConverterName}",
                $"Result    : {Message}");
        }

        return string.Join(Environment.NewLine,
            "Tile Conversion",
            "===============",
            $"Tile ID     : {TileId}",
            $"Record Index: {Record.Index}",
            $"FileName    : {Record.FileName}",
            $"Offset      : {Record.Offset:N0}",
            $"Size        : {Record.Size:N0}",
            $"CanExtract  : {(Record.CanExtract ? "YES" : "NO")}",
            $"Format      : {Record.Format}",
            $"Kind        : {Candidate.Kind}",
            $"Extension   : {Candidate.Extension}",
            $"Candidate   : {Candidate.Description}",
            $"Converter   : {ConverterName}",
            $"Image       : {(Success ? "available" : "unavailable")}",
            $"Result      : {Message}");
    }
}

public static class TileResourceClassifier
{
    public static TileConversionCandidate Classify(IdxRecord record)
    {
        var extension = Path.GetExtension(record.FileName).ToLowerInvariant();
        return extension switch
        {
            ".png" or ".bmp" or ".jpg" or ".jpeg" or ".gif" => new TileConversionCandidate(
                TileResourceKind.DirectImage,
                extension,
                true,
                "Common image format supported by System.Drawing"),
            ".spr" => new TileConversionCandidate(
                TileResourceKind.Sprite,
                extension,
                false,
                "Lineage sprite resource; dedicated SPR converter is required"),
            ".img" => new TileConversionCandidate(
                TileResourceKind.RawImage,
                extension,
                false,
                "Lineage raw image resource; dedicated IMG converter is required"),
            ".til" => new TileConversionCandidate(
                TileResourceKind.Tile,
                extension,
                false,
                "Lineage tile resource; dedicated TIL converter is required"),
            ".tbt" => new TileConversionCandidate(
                TileResourceKind.TileTable,
                extension,
                false,
                "Lineage tile table resource; metadata parser is required"),
            ".txt" or ".dat" or ".ini" or ".csv" => new TileConversionCandidate(
                TileResourceKind.Text,
                extension,
                false,
                "Text-like resource; not an image tile"),
            _ => new TileConversionCandidate(
                TileResourceKind.Binary,
                extension,
                false,
                "Unknown binary tile resource; converter not selected")
        };
    }
}

public sealed class DefaultTileImageCache : ITileImageCache
{
    private readonly Dictionary<int, TileConversionResult> _results = new();
    private readonly TileResourceConverterRegistry _registry = TileResourceConverterRegistry.CreateDefault();

    public TileConversionResult GetTileImage(int tileId, TileResourceSet tileResourceSet)
    {
        if (_results.TryGetValue(tileId, out var cached))
        {
            return cached;
        }

        var result = ConvertTile(tileId, tileResourceSet);
        _results[tileId] = result;
        return result;
    }

    public string GetConverterListText()
    {
        return _registry.ToDisplayText();
    }

    private TileConversionResult ConvertTile(int tileId, TileResourceSet tileResourceSet)
    {
        var record = tileResourceSet.FindByTileId(tileId);
        if (record is null)
        {
            return TileConversionResult.NotFound(tileId, "Tile ID에 해당하는 IDX 레코드를 찾지 못했습니다.");
        }

        var candidate = TileResourceClassifier.Classify(record);
        var converter = _registry.Select(candidate, record);
        return converter.Convert(tileId, tileResourceSet, record, candidate);
    }
}
