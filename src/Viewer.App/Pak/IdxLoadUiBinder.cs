namespace Viewer.App.Pak;

public sealed record IdxLoadUiState(
    string IdxPath,
    IdxParseResult ParseResult,
    IReadOnlyList<IdxRecord> Records
)
{
    public string InfoText => IdxParseResultPresenter.BuildLoadInfo(IdxPath, ParseResult);

    public string LogText => IdxParseResultPresenter.BuildLogMessage(IdxPath, ParseResult);

    public string ProbeWarning => IdxParseResultPresenter.BuildProbeWarning(ParseResult);
}

public static class IdxLoadUiBinder
{
    public static IdxLoadUiState Load(string idxPath)
    {
        var parseResult = IdxParser.ParseDetailed(idxPath);
        return new IdxLoadUiState(idxPath, parseResult, parseResult.Records);
    }

    public static void FillListView(ListView list, IEnumerable<IdxRecord> records)
    {
        list.Items.Clear();
        foreach (var record in records)
        {
            var item = new ListViewItem(record.Index.ToString())
            {
                Tag = record
            };
            item.SubItems.Add(record.FileName);
            item.SubItems.Add(record.Size.ToString("N0"));
            item.SubItems.Add(record.Offset.ToString("N0"));
            item.SubItems.Add(record.CanExtract ? "YES" : "NO");
            item.SubItems.Add(record.Format);
            list.Items.Add(item);
        }
    }
}
