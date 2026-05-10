using Viewer.App.Pak;

namespace Viewer.App.Map;

public interface ITileResourceConverter
{
    string Name { get; }

    bool CanConvert(TileConversionCandidate candidate, IdxRecord record);

    TileConversionResult Convert(int tileId, TileResourceSet tileResourceSet, IdxRecord record, TileConversionCandidate candidate);
}

public sealed class DirectImageTileResourceConverter : ITileResourceConverter
{
    public string Name => "DirectImage";

    public bool CanConvert(TileConversionCandidate candidate, IdxRecord record)
    {
        return candidate.Kind == TileResourceKind.DirectImage && candidate.CanAttemptDirectImage;
    }

    public TileConversionResult Convert(int tileId, TileResourceSet tileResourceSet, IdxRecord record, TileConversionCandidate candidate)
    {
        if (!record.CanExtract)
        {
            return new TileConversionResult(tileId, record, candidate, false, null, Name, "레코드가 추출 가능 상태가 아닙니다.");
        }

        try
        {
            var data = PakExtractor.ReadBytes(tileResourceSet.PakPath, record);
            var image = ImageResourceDecoder.LoadBitmap(data);
            return new TileConversionResult(tileId, record, candidate, true, image, Name, "직접 이미지 변환 성공");
        }
        catch (Exception ex)
        {
            return new TileConversionResult(tileId, record, candidate, false, null, Name, "직접 이미지 변환 실패: " + ex.Message);
        }
    }
}

public sealed class L1TilTileResourceConverter : ITileResourceConverter
{
    private const int MinTilBlockBytes = 2;
    private const int MaxTilPreviewBytes = 1024 * 1024;

    public string Name => "L1TIL";

    public bool CanConvert(TileConversionCandidate candidate, IdxRecord record)
    {
        return candidate.Kind == TileResourceKind.Tile;
    }

    public TileConversionResult Convert(int tileId, TileResourceSet tileResourceSet, IdxRecord record, TileConversionCandidate candidate)
    {
        if (!record.CanExtract)
        {
            return new TileConversionResult(tileId, record, candidate, false, null, Name, "레코드가 추출 가능 상태가 아닙니다.");
        }

        if (record.Size < MinTilBlockBytes)
        {
            return new TileConversionResult(tileId, record, candidate, false, null, Name, "TIL block 후보 데이터가 너무 작습니다.");
        }

        if (record.Size > MaxTilPreviewBytes)
        {
            return new TileConversionResult(tileId, record, candidate, false, null, Name, $"TIL preview는 최대 {MaxTilPreviewBytes:N0} bytes까지만 시도합니다. 현재 리소스는 {record.Size:N0} bytes입니다.");
        }

        try
        {
            var data = PakExtractor.ReadBytes(tileResourceSet.PakPath, record);
            if (data.Length < MinTilBlockBytes)
            {
                return new TileConversionResult(tileId, record, candidate, false, null, Name, "TIL block 후보 데이터가 비어 있습니다.");
            }

            var blocks = L1TilBlockParser.ParseBlocks(data);
            if (blocks.Count == 0)
            {
                return new TileConversionResult(tileId, record, candidate, false, null, Name, "TIL block parser가 block을 찾지 못했습니다.");
            }

            var image = blocks.Count == 1
                ? L1ImageFormatDecoder.RenderTilBlock(blocks[0].Data)
                : L1ImageFormatDecoder.RenderTilSheet(blocks.Select(block => block.Data).ToList());
            var summary = L1TilBlockParser.BuildSummary(blocks, data.Length);
            return new TileConversionResult(tileId, record, candidate, true, image, Name, "L1 TIL preview 생성 성공." + Environment.NewLine + summary);
        }
        catch (Exception ex)
        {
            return new TileConversionResult(tileId, record, candidate, false, null, Name, "L1 TIL preview 실패: " + ex.Message);
        }
    }
}

public sealed class RawByteDiagnosticTileResourceConverter : ITileResourceConverter
{
    private const int MaxDiagnosticBytes = 1024 * 1024;
    private const int MaxDiagnosticHeight = 512;

    public string Name => "RawByteDiagnostic";

    public bool CanConvert(TileConversionCandidate candidate, IdxRecord record)
    {
        return candidate.Kind is TileResourceKind.Tile or TileResourceKind.RawImage;
    }

    public TileConversionResult Convert(int tileId, TileResourceSet tileResourceSet, IdxRecord record, TileConversionCandidate candidate)
    {
        if (!record.CanExtract)
        {
            return new TileConversionResult(tileId, record, candidate, false, null, Name, "레코드가 추출 가능 상태가 아닙니다.");
        }

        if (record.Size > MaxDiagnosticBytes)
        {
            return new TileConversionResult(
                tileId,
                record,
                candidate,
                false,
                null,
                Name,
                $"Raw Byte 진단 이미지는 최대 {MaxDiagnosticBytes:N0} bytes까지만 허용합니다. 현재 리소스는 {record.Size:N0} bytes입니다.");
        }

        try
        {
            var data = PakExtractor.ReadBytes(tileResourceSet.PakPath, record);
            if (data.Length == 0)
            {
                return new TileConversionResult(tileId, record, candidate, false, null, Name, "빈 리소스입니다.");
            }

            var bitmap = BuildDiagnosticBitmap(data);
            return new TileConversionResult(
                tileId,
                record,
                candidate,
                true,
                bitmap,
                Name,
                "실제 TIL/IMG 렌더링이 아닌 Raw Byte 진단 이미지입니다. 데이터 패턴 확인용으로만 사용하세요.");
        }
        catch (Exception ex)
        {
            return new TileConversionResult(tileId, record, candidate, false, null, Name, "Raw Byte 진단 이미지 생성 실패: " + ex.Message);
        }
    }

    private static Bitmap BuildDiagnosticBitmap(byte[] data)
    {
        var width = GuessWidth(data.Length);
        var height = Math.Max(1, (int)Math.Ceiling(data.Length / (double)width));
        height = Math.Min(height, MaxDiagnosticHeight);

        var bitmap = new Bitmap(width, height);
        var max = Math.Min(data.Length, width * height);
        for (var i = 0; i < max; i++)
        {
            var value = data[i];
            var x = i % width;
            var y = i / width;
            bitmap.SetPixel(x, y, Color.FromArgb(value, value, value));
        }

        return bitmap;
    }

    private static int GuessWidth(int length)
    {
        if (length % 128 == 0)
        {
            return 128;
        }

        if (length % 64 == 0)
        {
            return 64;
        }

        if (length % 48 == 0)
        {
            return 48;
        }

        if (length % 32 == 0)
        {
            return 32;
        }

        if (length % 24 == 0)
        {
            return 24;
        }

        return 64;
    }
}

public sealed class PlaceholderTileResourceConverter : ITileResourceConverter
{
    private readonly TileResourceKind _kind;
    private readonly string _name;
    private readonly string _message;

    public PlaceholderTileResourceConverter(TileResourceKind kind, string name, string message)
    {
        _kind = kind;
        _name = name;
        _message = message;
    }

    public string Name => _name;

    public bool CanConvert(TileConversionCandidate candidate, IdxRecord record)
    {
        return candidate.Kind == _kind;
    }

    public TileConversionResult Convert(int tileId, TileResourceSet tileResourceSet, IdxRecord record, TileConversionCandidate candidate)
    {
        return new TileConversionResult(tileId, record, candidate, false, null, Name, _message);
    }
}

public sealed class UnsupportedTileResourceConverter : ITileResourceConverter
{
    public string Name => "Unsupported";

    public bool CanConvert(TileConversionCandidate candidate, IdxRecord record)
    {
        return true;
    }

    public TileConversionResult Convert(int tileId, TileResourceSet tileResourceSet, IdxRecord record, TileConversionCandidate candidate)
    {
        return new TileConversionResult(tileId, record, candidate, false, null, Name, candidate.Description);
    }
}

public sealed class TileResourceConverterRegistry
{
    private readonly List<ITileResourceConverter> _converters;

    public TileResourceConverterRegistry(IEnumerable<ITileResourceConverter> converters)
    {
        _converters = converters.ToList();
    }

    public static TileResourceConverterRegistry CreateDefault()
    {
        return new TileResourceConverterRegistry(new ITileResourceConverter[]
        {
            new DirectImageTileResourceConverter(),
            new L1TilTileResourceConverter(),
            new RawByteDiagnosticTileResourceConverter(),
            new PlaceholderTileResourceConverter(TileResourceKind.Sprite, "SPR", "SPR 변환기는 아직 구현되지 않았습니다. list.spr/프레임 구조 연동이 필요합니다."),
            new PlaceholderTileResourceConverter(TileResourceKind.TileTable, "TBT", "TBT는 이미지가 아닌 타일 테이블 후보입니다. 별도 메타데이터 파서가 필요합니다."),
            new PlaceholderTileResourceConverter(TileResourceKind.Text, "Text", "텍스트 리소스는 타일 이미지 변환 대상이 아닙니다."),
            new UnsupportedTileResourceConverter()
        });
    }

    public IReadOnlyList<ITileResourceConverter> Converters => _converters;

    public ITileResourceConverter Select(TileConversionCandidate candidate, IdxRecord record)
    {
        return _converters.First(converter => converter.CanConvert(candidate, record));
    }

    public TileConversionResult ConvertWithFallback(int tileId, TileResourceSet tileResourceSet, IdxRecord record, TileConversionCandidate candidate)
    {
        var primary = Select(candidate, record);
        var primaryResult = primary.Convert(tileId, tileResourceSet, record, candidate);
        if (primaryResult.Success || candidate.Kind != TileResourceKind.Tile || primary.Name == "RawByteDiagnostic")
        {
            return primaryResult;
        }

        var fallback = _converters.FirstOrDefault(converter => converter.Name == "RawByteDiagnostic" && converter.CanConvert(candidate, record));
        if (fallback is null)
        {
            return primaryResult;
        }

        var fallbackResult = fallback.Convert(tileId, tileResourceSet, record, candidate);
        if (!fallbackResult.Success)
        {
            return primaryResult.WithMessagePrefix("Primary converter failed and RawByte fallback also failed.");
        }

        return fallbackResult.WithMessagePrefix("Primary L1TIL converter failed; RawByte diagnostic fallback was used." + Environment.NewLine + primaryResult.Message);
    }

    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "Registered Tile Converters",
            "==========================",
            _converters.Select(converter => "- " + converter.Name));
    }
}
