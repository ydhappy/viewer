namespace Viewer.App.Pak;

public sealed record SpriteFrameDecodeRequest(
    byte[] Data,
    SpriteHeaderAnalysis Analysis,
    SpriteRawPreviewOptions PreviewOptions
);

public sealed record SpriteFrameDecodeResult(
    bool Success,
    Bitmap? Bitmap,
    string DecoderName,
    string Message,
    SpriteRawPreviewResult? RawPreview
) : IDisposable
{
    public string ToDisplayText()
    {
        var lines = new List<string>
        {
            "SPR Frame Decode Result",
            "=======================",
            $"Success: {(Success ? "YES" : "NO")}",
            $"Decoder: {DecoderName}",
            $"Message: {Message}"
        };

        if (RawPreview is not null)
        {
            lines.Add(string.Empty);
            lines.Add(RawPreview.ToDisplayText());
        }

        return string.Join(Environment.NewLine, lines);
    }

    public void Dispose()
    {
        Bitmap?.Dispose();
    }
}

public interface ISpriteFrameDecoder
{
    string Name { get; }

    bool CanDecode(SpriteFrameDecodeRequest request);

    SpriteFrameDecodeResult Decode(SpriteFrameDecodeRequest request);
}

public sealed class RawPreviewSpriteFrameDecoder : ISpriteFrameDecoder
{
    public string Name => "RawPreview";

    public bool CanDecode(SpriteFrameDecodeRequest request)
    {
        return request.Data.Length > 0;
    }

    public SpriteFrameDecodeResult Decode(SpriteFrameDecodeRequest request)
    {
        try
        {
            var preview = SpriteRawPreviewBuilder.Build(request.Data, request.Analysis, request.PreviewOptions);
            var bitmap = SpriteRawPreviewBuilder.ApplyZoom(preview.Bitmap, preview.Zoom);
            return new SpriteFrameDecodeResult(
                true,
                bitmap,
                Name,
                "실제 SPR 디코더가 아니라 raw preview fallback decoder입니다.",
                preview);
        }
        catch (Exception ex)
        {
            return new SpriteFrameDecodeResult(false, null, Name, "Raw preview decode failed: " + ex.Message, null);
        }
    }
}

public sealed class PlaceholderSpriteFrameDecoder : ISpriteFrameDecoder
{
    public string Name => "Placeholder";

    public bool CanDecode(SpriteFrameDecodeRequest request)
    {
        return true;
    }

    public SpriteFrameDecodeResult Decode(SpriteFrameDecodeRequest request)
    {
        return new SpriteFrameDecodeResult(
            false,
            null,
            Name,
            "실제 SPR 프레임 디코더는 아직 연결되지 않았습니다.",
            null);
    }
}

public sealed class SpriteFrameDecoderRegistry
{
    private readonly List<ISpriteFrameDecoder> _decoders;

    public SpriteFrameDecoderRegistry(IEnumerable<ISpriteFrameDecoder> decoders)
    {
        _decoders = decoders.ToList();
    }

    public static SpriteFrameDecoderRegistry CreateDefault()
    {
        return new SpriteFrameDecoderRegistry(new ISpriteFrameDecoder[]
        {
            new RawPreviewSpriteFrameDecoder(),
            new PlaceholderSpriteFrameDecoder()
        });
    }

    public IReadOnlyList<ISpriteFrameDecoder> Decoders => _decoders;

    public ISpriteFrameDecoder Select(SpriteFrameDecodeRequest request)
    {
        return _decoders.First(decoder => decoder.CanDecode(request));
    }

    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "Registered SPR Frame Decoders",
            "============================",
            _decoders.Select(decoder => "- " + decoder.Name));
    }
}
