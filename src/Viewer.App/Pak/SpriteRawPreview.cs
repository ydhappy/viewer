namespace Viewer.App.Pak;

public sealed record SpriteRawPreviewResult(
    Bitmap Bitmap,
    int Offset,
    int BytesUsed,
    int Width,
    int Height,
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
            $"Message   : {Message}");
    }
}

public static class SpriteRawPreviewBuilder
{
    private const int MaxPreviewBytes = 256 * 1024;
    private const int MaxHeight = 512;

    public static SpriteRawPreviewResult Build(byte[] data, SpriteHeaderAnalysis analysis)
    {
        if (data.Length == 0)
        {
            return BuildEmpty("empty SPR resource");
        }

        var offset = GuessPayloadOffset(data, analysis);
        if (offset >= data.Length)
        {
            offset = 0;
        }

        var available = data.Length - offset;
        var bytesToUse = Math.Min(available, MaxPreviewBytes);
        if (analysis.CandidateFrameBytes is > 0)
        {
            bytesToUse = Math.Min(bytesToUse, analysis.CandidateFrameBytes.Value);
        }

        if (bytesToUse <= 0)
        {
            return BuildEmpty("no payload bytes available for preview");
        }

        var width = GuessWidth(bytesToUse);
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
            "실제 SPR 렌더링이 아닌 후보 payload 회색조 preview입니다.");
    }

    private static SpriteRawPreviewResult BuildEmpty(string message)
    {
        var bitmap = new Bitmap(1, 1);
        bitmap.SetPixel(0, 0, Color.Black);
        return new SpriteRawPreviewResult(bitmap, 0, 0, 1, 1, message);
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
}
