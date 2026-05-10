using System.Text;

namespace Viewer.App.Pak;

public static class IdxParser
{
    public static List<IdxRecord> Parse(string idxPath)
    {
        if (!File.Exists(idxPath))
        {
            throw new FileNotFoundException("IDX 파일을 찾을 수 없습니다.", idxPath);
        }

        var bytes = File.ReadAllBytes(idxPath);
        var records = new List<IdxRecord>();

        // 1차 기본 파서:
        // 일부 IDX는 고정 길이 레코드가 아니므로, 우선 안전하게 바이너리 크기와 파일명을 표시한다.
        // 이후 PakViewer의 L1PakTools.IndexRecord 로직을 기능별로 흡수해 확장한다.
        records.Add(new IdxRecord(
            Index: 1,
            FileName: Path.GetFileName(idxPath),
            Size: bytes.Length,
            Offset: 0));

        TryParseAsciiLikeRecords(bytes, records);
        return records;
    }

    private static void TryParseAsciiLikeRecords(byte[] bytes, List<IdxRecord> records)
    {
        var text = Encoding.UTF8.GetString(bytes);
        var lines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        var nextIndex = records.Count + 1;

        foreach (var line in lines.Take(1000))
        {
            if (!line.Contains('.'))
            {
                continue;
            }

            var fileName = line.Trim();
            if (fileName.Length is < 3 or > 260)
            {
                continue;
            }

            records.Add(new IdxRecord(nextIndex++, fileName, 0, 0));
        }
    }
}
