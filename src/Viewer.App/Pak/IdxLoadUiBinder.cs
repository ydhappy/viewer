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
        EnsureCompressionColumns(list);
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
            item.SubItems.Add(record.Compression.ToString());
            item.SubItems.Add(record.CompressedSize?.ToString("N0") ?? "-");
            list.Items.Add(item);
        }
    }

    private static void EnsureCompressionColumns(ListView list)
    {
        if (!HasColumn(list, "Compression"))
        {
            list.Columns.Add("Compression", 95, HorizontalAlignment.Right);
        }

        if (!HasColumn(list, "Packed", true))
        {
            list.Columns.Add("Packed", 110, HorizontalAlignment.Right);
        }
    }

    private static bool HasColumn(ListView list, string text, bool startsWith = false)
    {
        foreach (ColumnHeader column in list.Columns)
        {
            if (startsWith)
            {
                if (column.Text.StartsWith(text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            else if (column.Text.Equals(text, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
