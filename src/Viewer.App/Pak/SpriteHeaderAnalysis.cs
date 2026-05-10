namespace Viewer.App.Pak;

public sealed record SpriteHeaderAnalysis(
    int Length,
    string Signature,
    int? CandidateFrameCount,
    int? CandidateDirectionCount,
    int? CandidatePaletteSize,
    int? CandidateFrameBytes,
    string Confidence,
    IReadOnlyList<string> Notes
)
{
    public string ToDisplayText()
    {
        var lines = new List<string>
        {
            "SPR Header Candidate Analysis",
            "=============================",
            $"Length              : {Length:N0} bytes",
            $"Signature           : {Signature}",
            $"Frame Count 후보   : {FormatNullable(CandidateFrameCount)}",
            $"Direction Count 후보: {FormatNullable(CandidateDirectionCount)}",
            $"Palette Size 후보   : {FormatNullable(CandidatePaletteSize)}",
            $"Frame Bytes 후보    : {FormatNullable(CandidateFrameBytes)}",
            $"Confidence          : {Confidence}",
            string.Empty,
            "Notes",
            "-----"
        };

        if (Notes.Count == 0)
        {
            lines.Add("- no notes");
        }
        else
        {
            lines.AddRange(Notes.Select(note => "- " + note));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatNullable(int? value)
    {
        return value is null ? "-" : value.Value.ToString("N0");
    }
}

public static class SpriteHeaderAnalyzer
{
    public static SpriteHeaderAnalysis Analyze(byte[] data)
    {
        var notes = new List<string>();
        if (data.Length == 0)
        {
            return new SpriteHeaderAnalysis(0, "empty", null, null, null, null, "none", new[] { "empty resource" });
        }

        var signature = BuildSignature(data, 16);
        var firstU16 = ReadUInt16(data, 0);
        var secondU16 = ReadUInt16(data, 2);
        var firstU32 = ReadUInt32(data, 0);
        var secondU32 = ReadUInt32(data, 4);

        int? frameCount = null;
        int? directionCount = null;
        int? paletteSize = null;
        int? frameBytes = null;
        var confidence = "low";

        if (firstU16 is > 0 and <= 4096)
        {
            frameCount = firstU16;
            notes.Add("첫 2바이트를 frame count 후보로 해석할 수 있습니다.");
        }

        if (secondU16 is 1 or 2 or 4 or 8 or 16)
        {
            directionCount = secondU16;
            notes.Add("두 번째 2바이트가 방향 수 후보처럼 보입니다.");
        }

        var commonPalette = GuessPaletteSize(data.Length);
        if (commonPalette is not null)
        {
            paletteSize = commonPalette;
            notes.Add($"파일 크기 기준 {commonPalette:N0} bytes palette 후보를 고려할 수 있습니다.");
        }

        if (frameCount is > 0)
        {
            var payloadStartCandidates = new[] { 4, 8, 16, 32 };
            foreach (var start in payloadStartCandidates)
            {
                if (data.Length > start)
                {
                    var candidateBytes = (data.Length - start - (paletteSize ?? 0)) / frameCount.Value;
                    if (candidateBytes > 0)
                    {
                        frameBytes = candidateBytes;
                        notes.Add($"payload start {start} 기준 frame byte 후보를 계산했습니다.");
                        break;
                    }
                }
            }
        }

        if (firstU32 is > 0 and < 10_000_000)
        {
            notes.Add($"첫 4바이트 UInt32 후보값: {firstU32:N0}");
        }

        if (secondU32 is > 0 and < 10_000_000)
        {
            notes.Add($"두 번째 4바이트 UInt32 후보값: {secondU32:N0}");
        }

        if (frameCount is not null && frameBytes is not null)
        {
            confidence = directionCount is not null ? "medium" : "low-medium";
        }

        notes.Add("이 분석은 실제 SPR 구조 확정이 아니라 디코더 이식 전 후보 추정입니다.");
        notes.Add("실제 프레임/팔레트/압축 구조는 원본 디코더 흡수 후 확정해야 합니다.");

        return new SpriteHeaderAnalysis(
            data.Length,
            signature,
            frameCount,
            directionCount,
            paletteSize,
            frameBytes,
            confidence,
            notes);
    }

    private static int? GuessPaletteSize(int length)
    {
        if (length > 1024 && length % 768 == 0)
        {
            return 768;
        }

        if (length > 1024 && length % 1024 == 0)
        {
            return 1024;
        }

        if (length > 2048 && (length - 768) > 0)
        {
            return 768;
        }

        return null;
    }

    private static ushort? ReadUInt16(byte[] data, int offset)
    {
        if (data.Length < offset + 2)
        {
            return null;
        }

        return BitConverter.ToUInt16(data, offset);
    }

    private static uint? ReadUInt32(byte[] data, int offset)
    {
        if (data.Length < offset + 4)
        {
            return null;
        }

        return BitConverter.ToUInt32(data, offset);
    }

    private static string BuildSignature(byte[] data, int maxBytes)
    {
        return string.Join(' ', data.Take(Math.Min(maxBytes, data.Length)).Select(value => value.ToString("X2")));
    }
}
