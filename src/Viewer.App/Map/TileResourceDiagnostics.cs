using System.Text;
using Viewer.App.Pak;

namespace Viewer.App.Map;

public sealed record TileResourceDiagnostics(
    IdxRecord Record,
    TileConversionCandidate Candidate,
    int BytesRead,
    string Summary,
    string HexPreview
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "Tile Resource Diagnostics",
            "=========================",
            $"FileName : {Record.FileName}",
            $"Kind     : {Candidate.Kind}",
            $"Ext      : {Candidate.Extension}",
            $"Offset   : {Record.Offset:N0}",
            $"Size     : {Record.Size:N0}",
            $"Read     : {BytesRead:N0} bytes",
            $"Summary  : {Summary}",
            string.Empty,
            "HEX Preview",
            "-----------",
            HexPreview);
    }
}

public static class TileResourceDiagnosticsAnalyzer
{
    public static TileResourceDiagnostics Analyze(TileResourceSet tileResourceSet, IdxRecord record, int maxBytes = 512)
    {
        var candidate = TileResourceClassifier.Classify(record);

        if (!record.CanExtract)
        {
            return new TileResourceDiagnostics(
                record,
                candidate,
                0,
                "레코드가 추출 가능 상태가 아니므로 바이트를 읽지 않았습니다.",
                string.Empty);
        }

        try
        {
            var data = PakExtractor.ReadBytes(tileResourceSet.PakPath, record);
            var previewLength = Math.Min(data.Length, maxBytes);
            var preview = data.Take(previewLength).ToArray();
            return new TileResourceDiagnostics(
                record,
                candidate,
                previewLength,
                BuildSummary(candidate, data),
                BuildHexPreview(preview));
        }
        catch (Exception ex)
        {
            return new TileResourceDiagnostics(
                record,
                candidate,
                0,
                "리소스 바이트 읽기 실패: " + ex.Message,
                string.Empty);
        }
    }

    private static string BuildSummary(TileConversionCandidate candidate, byte[] data)
    {
        if (data.Length == 0)
        {
            return "empty resource";
        }

        var signature = GetSignature(data);
        return candidate.Kind switch
        {
            TileResourceKind.DirectImage => "일반 이미지 후보입니다. Signature=" + signature,
            TileResourceKind.Tile => "TIL 타일 후보입니다. 현재는 헤더/바이트 진단만 지원합니다. Signature=" + signature,
            TileResourceKind.RawImage => "IMG 원시 이미지 후보입니다. 폭/높이/팔레트 구조 확인이 필요합니다. Signature=" + signature,
            TileResourceKind.Sprite => "SPR 스프라이트 후보입니다. 프레임/방향/list.spr 연동 분석이 필요합니다. Signature=" + signature,
            TileResourceKind.TileTable => "TBT 타일 테이블 후보입니다. 이미지가 아닌 메타데이터일 가능성이 높습니다. Signature=" + signature,
            TileResourceKind.Text => "텍스트 후보입니다. 이미지 변환 대상은 아닙니다. Signature=" + signature,
            _ => "알 수 없는 바이너리 후보입니다. Signature=" + signature
        };
    }

    private static string GetSignature(byte[] data)
    {
        if (data.Length >= 4)
        {
            if (data[0] == 0x89 && data[1] == 0x50 && data[2] == 0x4E && data[3] == 0x47)
            {
                return "PNG";
            }

            if (data[0] == 0x42 && data[1] == 0x4D)
            {
                return "BMP";
            }

            if (data[0] == 0xFF && data[1] == 0xD8)
            {
                return "JPEG";
            }

            if (data[0] == 0x47 && data[1] == 0x49 && data[2] == 0x46)
            {
                return "GIF";
            }
        }

        return string.Join(' ', data.Take(Math.Min(8, data.Length)).Select(value => value.ToString("X2")));
    }

    private static string BuildHexPreview(byte[] data)
    {
        if (data.Length == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        for (var offset = 0; offset < data.Length; offset += 16)
        {
            builder.Append(offset.ToString("X8"));
            builder.Append("  ");

            var lineLength = Math.Min(16, data.Length - offset);
            for (var i = 0; i < 16; i++)
            {
                if (i < lineLength)
                {
                    builder.Append(data[offset + i].ToString("X2"));
                    builder.Append(' ');
                }
                else
                {
                    builder.Append("   ");
                }
            }

            builder.Append(' ');
            for (var i = 0; i < lineLength; i++)
            {
                var value = data[offset + i];
                builder.Append(value is >= 32 and <= 126 ? (char)value : '.');
            }

            builder.AppendLine();
        }

        return builder.ToString();
    }
}
