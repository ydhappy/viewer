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
            using var stream = new MemoryStream(data);
            using var image = Image.FromStream(stream);
            return new TileConversionResult(tileId, record, candidate, true, new Bitmap(image), Name, "직접 이미지 변환 성공");
        }
        catch (Exception ex)
        {
            return new TileConversionResult(tileId, record, candidate, false, null, Name, "직접 이미지 변환 실패: " + ex.Message);
        }
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
            new PlaceholderTileResourceConverter(TileResourceKind.Tile, "TIL", "TIL 변환기는 아직 구현되지 않았습니다. 다음 단계에서 원본 타일 포맷을 흡수합니다."),
            new PlaceholderTileResourceConverter(TileResourceKind.RawImage, "IMG", "IMG 변환기는 아직 구현되지 않았습니다. 원본 IMG 포맷 해석이 필요합니다."),
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

    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "Registered Tile Converters",
            "==========================",
            _converters.Select(converter => "- " + converter.Name));
    }
}
