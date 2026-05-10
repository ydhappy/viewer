using Viewer.App.Pak;

namespace Viewer.App.Map;

public sealed record TileResourceSet(
    string IdxPath,
    string PakPath,
    int TotalRecords,
    int ExtractableRecords,
    long IdxSize,
    long PakSize,
    IReadOnlyList<IdxRecord> Records
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "Tile Resource",
            "=============",
            $"IDX Path    : {IdxPath}",
            $"PAK Path    : {PakPath}",
            $"IDX Size    : {IdxSize:N0} bytes",
            $"PAK Size    : {PakSize:N0} bytes",
            $"Records     : {TotalRecords:N0}",
            $"Extractable : {ExtractableRecords:N0}",
            string.Empty,
            "※ 12차에서는 Tile.idx 레코드 목록/검색과 캐시 연결 준비까지 처리합니다.",
            "※ 실제 Tile 이미지 변환은 SPR/IMG/TIL 변환기 연결 후 처리합니다.");
    }

    public IdxRecord? FindByTileId(int tileId)
    {
        if (tileId <= 0 || Records.Count == 0)
        {
            return null;
        }

        var direct = Records.FirstOrDefault(record => record.Index == tileId);
        if (direct is not null)
        {
            return direct;
        }

        var asText = tileId.ToString();
        return Records.FirstOrDefault(record =>
            Path.GetFileNameWithoutExtension(record.FileName).Equals(asText, StringComparison.OrdinalIgnoreCase) ||
            record.FileName.Contains(asText, StringComparison.OrdinalIgnoreCase));
    }

    public static TileResourceSet Load(string idxPath)
    {
        if (!File.Exists(idxPath))
        {
            throw new FileNotFoundException("Tile IDX 파일을 찾을 수 없습니다.", idxPath);
        }

        var pakPath = PakExtractor.ResolvePakPath(idxPath);
        var records = IdxParser.Parse(idxPath);

        return new TileResourceSet(
            IdxPath: idxPath,
            PakPath: pakPath,
            TotalRecords: records.Count,
            ExtractableRecords: records.Count(r => r.CanExtract),
            IdxSize: new FileInfo(idxPath).Length,
            PakSize: File.Exists(pakPath) ? new FileInfo(pakPath).Length : 0,
            Records: records);
    }
}
