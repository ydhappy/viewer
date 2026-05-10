namespace Viewer.App.Pak;

public sealed record SpriteRawPreviewOptions(
    int? Width,
    int? Offset,
    int FrameIndex,
    double Zoom
)
{
    public static SpriteRawPreviewOptions Auto { get; } = new(null, null, 0, 1.0);
}

public sealed record SpriteRawPreviewResult(
    Bitmap Bitmap,
    int Offset,
    int BytesUsed,
    int Width,
    int Height,
    int FrameIndex,
    double Zoom,
    string Message
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "SPR Raw Preview",
            "===============",
            $"Offset    : {Offset:N0}",
            $"Bytes Used: {BytesUsed:N0}",
            $"Width     : {Width:N0}",
            $"Height    : {Height:N0}",
            $"FrameIndex: {FrameIndex:N0}",
            $"Zoom      : {Zoom:0.00}x",
            $"Message   : {Message}");
    }
}

public static class SpriteRawPreviewBuilder
{
    private const int MaxPreviewBytes = 256 * 1024;
    private const int MaxHeight = 512;

    public static SpriteRawPreviewResult Build(byte[] data, SpriteHeaderAnalysis analysis)
    {
        return Build(data, analysis, SpriteRawPreviewOptions.Auto);
    }

    public static SpriteRawPreviewResult Build(byte[] data, SpriteHeaderAnalysis analysis, SpriteRawPreviewOptions options)
    {
        if (data.Length == 0)
        {
            return BuildEmpty("empty SPR resource", options);
        }

        var baseOffset = options.Offset ?? GuessPayloadOffset(data, analysis);
        if (baseOffset < 0)
        {
            baseOffset = 0;
        }

        if (baseOffset >= data.Length)
        {
            baseOffset = 0;
        }

        var frameBytes = analysis.CandidateFrameBytes is > 0 ? analysis.CandidateFrameBytes.Value : 0;
        var frameIndex = Math.Max(0, options.FrameIndex);
        var offset = baseOffset;
        if (frameBytes > 0 && frameIndex > 0)
        {
            var frameOffset = baseOffset + frameBytes * frameIndex;
            if (frameOffset < data.Length)
            {
                offset = frameOffset;
            }
        }

        var available = data.Length - offset;
        var bytesToUse = Math.Min(available, MaxPreviewBytes);
        if (frameBytes > 0)
        {
            bytesToUse = Math.Min(bytesToUse, frameBytes);
        }

        if (bytesToUse <= 0)
        {
            return BuildEmpty("no payload bytes available for preview", options);
        }

        var width = options.Width is > 0 ? options.Width.Value : GuessWidth(bytesToUse);
        width = Math.Clamp(width, 1, 2048);
        var height = Math.Max(1, (int)Math.Ceiling(bytesToUse / (double)width));
        height = Math.Min(height, MaxHeight);
        bytesToUse = Math.Min(bytesToUse, width * height);

        var bitmap = new Bitmap(width, height);
        for (var i = 0; i < bytesToUse; i++)
        {
            var value = data[offset + i];
            var x = i % width;
            var y = i / width;
            bitmap.SetPixel(x, y, Color.FromArgb(value, value, value));
        }

        return new SpriteRawPreviewResult(
            bitmap,
            offset,
            bytesToUse,
            width,
            height,
            frameIndex,
            NormalizeZoom(options.Zoom),
            "실제 SPR 렌더링이 아닌 후보 payload 회색조 preview입니다.");
    }

    public static Bitmap ApplyZoom(Bitmap source, double zoom)
    {
        var normalizedZoom = NormalizeZoom(zoom);
        if (Math.Abs(normalizedZoom - 1.0) < 0.001)
        {
            return new Bitmap(source);
        }

        var width = Math.Max(1, (int)Math.Round(source.Width * normalizedZoom));
        var height = Math.Max(1, (int)Math.Round(source.Height * normalizedZoom));
        var scaled = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(scaled);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;
        graphics.DrawImage(source, new Rectangle(0, 0, width, height));
        return scaled;
    }

    private static SpriteRawPreviewResult BuildEmpty(string message, SpriteRawPreviewOptions options)
    {
        var bitmap = new Bitmap(1, 1);
        bitmap.SetPixel(0, 0, Color.Black);
        return new SpriteRawPreviewResult(bitmap, 0, 0, 1, 1, Math.Max(0, options.FrameIndex), NormalizeZoom(options.Zoom), message);
    }

    private static int GuessPayloadOffset(byte[] data, SpriteHeaderAnalysis analysis)
    {
        var candidates = new[] { 32, 16, 8, 4, 0 };
        foreach (var candidate in candidates)
        {
            if (candidate < data.Length)
            {
                return candidate;
            }
        }

        return 0;
    }

    private static int GuessWidth(int bytes)
    {
        if (bytes % 128 == 0)
        {
            return 128;
        }

        if (bytes % 96 == 0)
        {
            return 96;
        }

        if (bytes % 64 == 0)
        {
            return 64;
        }

        if (bytes % 48 == 0)
        {
            return 48;
        }

        if (bytes % 32 == 0)
        {
            return 32;
        }

        return 64;
    }

    private static double NormalizeZoom(double zoom)
    {
        if (double.IsNaN(zoom) || double.IsInfinity(zoom) || zoom <= 0)
        {
            return 1.0;
        }

        return Math.Clamp(zoom, 0.25, 16.0);
    }
}
