namespace Viewer.App.Pak;

public sealed class SpriteResourcePanel : UserControl
{
    private readonly TextBox _filterBox = new();
    private readonly Button _filterButton = new();
    private readonly ListView _entryList = new();
    private readonly TextBox _detailBox = new();
    private readonly Panel _renderPlaceholder = new();

    private SpriteListCatalog? _catalog;
    private IReadOnlyList<IdxRecord> _records = Array.Empty<IdxRecord>();
    private IReadOnlyList<SpriteListEntry> _visibleEntries = Array.Empty<SpriteListEntry>();

    public SpriteResourcePanel()
    {
        Dock = DockStyle.Fill;

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 42));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(8)
        };

        _filterBox.Width = 220;
        _filterBox.PlaceholderText = "Sprite ID / 이름 / 그룹 / 액션";
        _filterButton.Text = "검색";
        _filterButton.AutoSize = true;
        toolbar.Controls.Add(new Label { Text = "Sprite 검색", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        toolbar.Controls.Add(_filterBox);
        toolbar.Controls.Add(_filterButton);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 620
        };

        _entryList.Dock = DockStyle.Fill;
        _entryList.View = View.Details;
        _entryList.FullRowSelect = true;
        _entryList.GridLines = true;
        _entryList.MultiSelect = false;
        _entryList.Columns.Add("Index", 70, HorizontalAlignment.Right);
        _entryList.Columns.Add("SpriteId", 90, HorizontalAlignment.Right);
        _entryList.Columns.Add("Name", 180, HorizontalAlignment.Left);
        _entryList.Columns.Add("Group", 120, HorizontalAlignment.Left);
        _entryList.Columns.Add("Action", 120, HorizontalAlignment.Left);
        _entryList.Columns.Add("Frame", 70, HorizontalAlignment.Right);

        var rightTabs = new TabControl { Dock = DockStyle.Fill };
        _detailBox.Dock = DockStyle.Fill;
        _detailBox.Multiline = true;
        _detailBox.ReadOnly = true;
        _detailBox.ScrollBars = ScrollBars.Both;
        _detailBox.Font = new Font(FontFamily.GenericMonospace, 10);
        _detailBox.Text = "PAK 탭에서 IDX와 list.spr를 열면 Sprite 목록과 SPR 레코드 매핑이 표시됩니다.";

        _renderPlaceholder.Dock = DockStyle.Fill;
        _renderPlaceholder.BackColor = Color.FromArgb(24, 24, 24);
        _renderPlaceholder.Paint += (_, e) => DrawRenderPlaceholder(e.Graphics);

        rightTabs.TabPages.Add(new TabPage("Detail") { Controls = { _detailBox } });
        rightTabs.TabPages.Add(new TabPage("Render") { Controls = { _renderPlaceholder } });

        split.Panel1.Controls.Add(_entryList);
        split.Panel2.Controls.Add(rightTabs);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(split, 0, 1);
        Controls.Add(layout);

        _filterButton.Click += (_, _) => ApplyFilter();
        _filterBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ApplyFilter();
            }
        };
        _entryList.SelectedIndexChanged += (_, _) => ShowSelectedEntry();
    }

    public void SetCatalog(SpriteListCatalog catalog)
    {
        _catalog = catalog;
        ApplyFilter();
    }

    public void SetRecords(IReadOnlyList<IdxRecord> records)
    {
        _records = records;
        ShowInitialState();
    }

    private void ApplyFilter()
    {
        if (_catalog is null)
        {
            ShowInitialState();
            return;
        }

        var query = _filterBox.Text.Trim();
        IEnumerable<SpriteListEntry> entries = _catalog.Entries;
        if (!string.IsNullOrWhiteSpace(query))
        {
            entries = entries.Where(entry => Matches(entry, query));
        }

        _visibleEntries = entries.Take(5000).ToList();
        _entryList.Items.Clear();
        foreach (var entry in _visibleEntries)
        {
            var item = new ListViewItem(entry.Index.ToString())
            {
                Tag = entry
            };
            item.SubItems.Add(entry.SpriteId.ToString());
            item.SubItems.Add(entry.Name);
            item.SubItems.Add(entry.Group);
            item.SubItems.Add(entry.Action);
            item.SubItems.Add(entry.Frame.ToString());
            _entryList.Items.Add(item);
        }

        _detailBox.Text = _catalog.ToDisplayText() + Environment.NewLine + Environment.NewLine +
            $"Visible Entries: {_entryList.Items.Count:N0}" + Environment.NewLine +
            $"SPR Records    : {CountSprRecords():N0}" + Environment.NewLine +
            "※ 목록은 UI 부하를 막기 위해 최대 5,000개까지만 표시합니다.";
        _renderPlaceholder.Invalidate();
    }

    private void ShowInitialState()
    {
        if (_catalog is null)
        {
            _entryList.Items.Clear();
            _detailBox.Text = string.Join(Environment.NewLine,
                "Sprite Resource",
                "===============",
                $"SPR Records: {CountSprRecords():N0}",
                string.Empty,
                "list.spr catalog is not loaded.",
                "PAK 탭에서 list.spr 열기를 먼저 실행하세요.");
            _renderPlaceholder.Invalidate();
            return;
        }

        ApplyFilter();
    }

    private bool Matches(SpriteListEntry entry, string query)
    {
        if (entry.SpriteId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return entry.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
               entry.Group.Contains(query, StringComparison.OrdinalIgnoreCase) ||
               entry.Action.Contains(query, StringComparison.OrdinalIgnoreCase);
    }

    private void ShowSelectedEntry()
    {
        if (_entryList.SelectedItems.Count != 1 || _entryList.SelectedItems[0].Tag is not SpriteListEntry entry)
        {
            return;
        }

        var record = FindSprRecord(entry);
        _detailBox.Text = BuildEntryDetail(entry, record);
        _renderPlaceholder.Invalidate();
    }

    private string BuildEntryDetail(SpriteListEntry entry, IdxRecord? record)
    {
        var lines = new List<string>
        {
            entry.ToDisplayText(),
            string.Empty,
            "SPR Record Mapping",
            "=================="
        };

        if (record is null)
        {
            lines.Add("No matching .spr PAK record found.");
            lines.Add("Mapping rule: record.Index, numeric filename, or filename name match.");
        }
        else
        {
            lines.Add($"Record Index: {record.Index}");
            lines.Add($"FileName    : {record.FileName}");
            lines.Add($"Offset      : {record.Offset:N0}");
            lines.Add($"Size        : {record.Size:N0}");
            lines.Add($"CanExtract  : {(record.CanExtract ? "YES" : "NO")}");
            lines.Add($"Format      : {record.Format}");
        }

        lines.Add(string.Empty);
        lines.Add("Render Placeholder");
        lines.Add("==================");
        lines.Add("실제 SPR 프레임 디코더는 아직 연결되지 않았습니다.");
        lines.Add("다음 단계에서 프레임/방향/액션 단위 디코더를 이식합니다.");

        return string.Join(Environment.NewLine, lines);
    }

    private IdxRecord? FindSprRecord(SpriteListEntry entry)
    {
        var sprRecords = _records.Where(record => Path.GetExtension(record.FileName).Equals(".spr", StringComparison.OrdinalIgnoreCase)).ToList();

        var byIndex = sprRecords.FirstOrDefault(record => record.Index == entry.SpriteId);
        if (byIndex is not null)
        {
            return byIndex;
        }

        var byNumericName = sprRecords.FirstOrDefault(record =>
            int.TryParse(Path.GetFileNameWithoutExtension(record.FileName), out var spriteId) && spriteId == entry.SpriteId);
        if (byNumericName is not null)
        {
            return byNumericName;
        }

        return sprRecords.FirstOrDefault(record =>
            Path.GetFileNameWithoutExtension(record.FileName).Equals(entry.Name, StringComparison.OrdinalIgnoreCase));
    }

    private int CountSprRecords()
    {
        return _records.Count(record => Path.GetExtension(record.FileName).Equals(".spr", StringComparison.OrdinalIgnoreCase));
    }

    private void DrawRenderPlaceholder(Graphics graphics)
    {
        graphics.Clear(_renderPlaceholder.BackColor);
        using var titleFont = new Font(FontFamily.GenericSansSerif, 13, FontStyle.Bold);
        using var font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
        using var titleBrush = new SolidBrush(Color.White);
        using var brush = new SolidBrush(Color.FromArgb(210, 210, 210));
        using var borderPen = new Pen(Color.FromArgb(120, 180, 180, 180));

        var rect = new Rectangle(32, 72, Math.Max(80, _renderPlaceholder.Width - 64), Math.Max(80, _renderPlaceholder.Height - 128));
        graphics.DrawRectangle(borderPen, rect);
        graphics.DrawString("SPR Renderer Placeholder", titleFont, titleBrush, 32, 24);
        graphics.DrawString("list.spr entry와 PAK .spr record 매핑은 준비되었습니다.", font, brush, 44, 92);
        graphics.DrawString("실제 프레임 디코딩/팔레트/방향별 렌더링은 다음 단계에서 연결합니다.", font, brush, 44, 118);
    }
}
