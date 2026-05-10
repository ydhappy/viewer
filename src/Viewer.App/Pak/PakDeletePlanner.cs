namespace Viewer.App.Pak;

public sealed record PakDeletePlanItem(
    string FileName,
    int Index,
    int Offset,
    int Size,
    bool CanDelete,
    string Reason
)
{
    public string ToDisplayLine()
    {
        return string.Join("\t", new[]
        {
            Index.ToString(),
            FileName,
            Offset.ToString(),
            Size.ToString(),
            CanDelete ? "DELETE" : "SKIP",
            Reason
        });
    }
}

public sealed record PakDeletePlan(
    string PakPath,
    IReadOnlyList<PakDeletePlanItem> Items
)
{
    public int DeleteCount => Items.Count(item => item.CanDelete);
    public int SkipCount => Items.Count(item => !item.CanDelete);
    public long DeleteBytes => Items.Where(item => item.CanDelete).Sum(item => (long)item.Size);

    public string ToDisplayText()
    {
        var lines = new List<string>
        {
            "PAK Delete Plan",
            "===============",
            $"PAK         : {PakPath}",
            $"Delete count: {DeleteCount:N0}",
            $"Skip count  : {SkipCount:N0}",
            $"Delete bytes: {DeleteBytes:N0}",
            string.Empty,
            "Index\tFileName\tOffset\tSize\tAction\tReason"
        };
        lines.AddRange(Items.Select(item => item.ToDisplayLine()));
        return string.Join(Environment.NewLine, lines);
    }
}

public static class PakDeletePlanner
{
    public static PakDeletePlan BuildPlan(string pakPath, IEnumerable<IdxRecord> records)
    {
        var pakSize = File.Exists(pakPath) ? new FileInfo(pakPath).Length : -1;
        var items = records.Select(record => BuildPlanItem(record, pakSize)).ToList();
        return new PakDeletePlan(pakPath, items);
    }

    private static PakDeletePlanItem BuildPlanItem(IdxRecord record, long pakSize)
    {
        if (!record.CanExtract)
        {
            return Skip(record, "record is not extractable");
        }

        if (record.Offset < 0 || record.Size <= 0)
        {
            return Skip(record, "invalid offset or size");
        }

        if (pakSize < 0)
        {
            return Skip(record, "pak file does not exist");
        }

        if (record.Offset + record.Size > pakSize)
        {
            return Skip(record, "record range exceeds pak file size");
        }

        if (record.Compression != 0 || record.CompressedSize is > 0)
        {
            return Skip(record, "compressed record delete requires rebuild flow");
        }

        return new PakDeletePlanItem(
            record.FileName,
            record.Index,
            record.Offset,
            record.Size,
            true,
            "safe raw record candidate");
    }

    private static PakDeletePlanItem Skip(IdxRecord record, string reason)
    {
        return new PakDeletePlanItem(
            record.FileName,
            record.Index,
            record.Offset,
            record.Size,
            false,
            reason);
    }
}
