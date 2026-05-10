using Viewer.App.Pak;

namespace Viewer.App.Map;

public sealed record TileResourceSet(
    string IdxPath,
    string PakPath,
    int TotalRecords,
    int ExtractableRecords,
    long IdxSize,
    long PakSize
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
            "※ 7차에서는 Tile.idx 상태 연결까지만 처리합니다.",
            "※ 실제 Tile 이미지 변환/캐시는 다음 렌더러 단계에서 연결합니다.");
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
            PakSize: File.Exists(pakPath) ? new FileInfo(pakPath).Length : 0);
    }
}
