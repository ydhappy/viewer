namespace Viewer.App.Map;

public sealed class TileResourcePanel : UserControl
{
    private readonly TextBox _searchBox = new();
    private readonly Button _searchButton = new();
    private readonly ListView _recordList = new();
    private readonly TextBox _detailBox = new();
    private readonly ITileImageCache _imageCache = new NullTileImageCache();

    private TileResourceSet? _tileResourceSet;

    public TileResourcePanel()
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

        _searchBox.Width = 120;
        _searchBox.PlaceholderText = "Tile ID";
        _searchButton.Text = "검색";
        _searchButton.AutoSize = true;
        toolbar.Controls.Add(new Label { Text = "Tile ID", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        toolbar.Controls.Add(_searchBox);
        toolbar.Controls.Add(_searchButton);

        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Vertical,
            SplitterDistance = 520
        };

        _recordList.Dock = DockStyle.Fill;
        _recordList.View = View.Details;
        _recordList.FullRowSelect = true;
        _recordList.GridLines = true;
        _recordList.MultiSelect = false;
        _recordList.Columns.Add("Index", 70, HorizontalAlignment.Right);
        _recordList.Columns.Add("FileName", 260, HorizontalAlignment.Left);
        _recordList.Columns.Add("Size", 110, HorizontalAlignment.Right);
        _recordList.Columns.Add("Offset", 110, HorizontalAlignment.Right);
        _recordList.Columns.Add("Extract", 70, HorizontalAlignment.Center);

        _detailBox.Dock = DockStyle.Fill;
        _detailBox.Multiline = true;
        _detailBox.ReadOnly = true;
        _detailBox.ScrollBars = ScrollBars.Both;
        _detailBox.Font = new Font(FontFamily.GenericMonospace, 10);
        _detailBox.Text = "Tile.idx를 열면 이곳에 타일 리소스 상태와 레코드 목록이 표시됩니다.";

        split.Panel1.Controls.Add(_recordList);
        split.Panel2.Controls.Add(_detailBox);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(split, 0, 1);
        Controls.Add(layout);

        _searchButton.Click += (_, _) => SearchTile();
        _searchBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SearchTile();
            }
        };
        _recordList.SelectedIndexChanged += (_, _) => ShowSelectedRecord();
    }

    public void SetTileResource(TileResourceSet tileResourceSet)
    {
        _tileResourceSet = tileResourceSet;
        _recordList.Items.Clear();

        foreach (var record in tileResourceSet.Records.Take(5000))
        {
            var item = new ListViewItem(record.Index.ToString())
            {
                Tag = record
            };
            item.SubItems.Add(record.FileName);
            item.SubItems.Add(record.Size.ToString("N0"));
            item.SubItems.Add(record.Offset.ToString("N0"));
            item.SubItems.Add(record.CanExtract ? "YES" : "NO");
            _recordList.Items.Add(item);
        }

        _detailBox.Text = tileResourceSet.ToDisplayText() + Environment.NewLine + Environment.NewLine +
            $"Displayed Records: {_recordList.Items.Count:N0}" + Environment.NewLine +
            "※ 목록은 과도한 UI 부하를 막기 위해 최대 5,000개까지만 표시합니다.";
    }

    private void SearchTile()
    {
        if (_tileResourceSet is null)
        {
            _detailBox.Text = "Tile.idx가 아직 로드되지 않았습니다.";
            return;
        }

        if (!int.TryParse(_searchBox.Text.Trim(), out var tileId))
        {
            _detailBox.Text = "검색할 Tile ID를 숫자로 입력하세요.";
            return;
        }

        var record = _tileResourceSet.FindByTileId(tileId);
        var hasImage = _imageCache.TryGetTileImage(tileId, _tileResourceSet, out _);
        var lookup = new TileRecordLookup(tileId, record, hasImage);
        _detailBox.Text = lookup.ToDisplayText();

        if (record is not null)
        {
            SelectRecord(record);
        }
    }

    private void SelectRecord(Viewer.App.Pak.IdxRecord record)
    {
        foreach (ListViewItem item in _recordList.Items)
        {
            if (item.Tag is Viewer.App.Pak.IdxRecord candidate && candidate.Index == record.Index)
            {
                item.Selected = true;
                item.EnsureVisible();
                _recordList.Focus();
                return;
            }
        }
    }

    private void ShowSelectedRecord()
    {
        if (_recordList.SelectedItems.Count != 1 || _recordList.SelectedItems[0].Tag is not Viewer.App.Pak.IdxRecord record)
        {
            return;
        }

        _detailBox.Text = string.Join(Environment.NewLine,
            "Tile Record",
            "===========",
            $"Index     : {record.Index}",
            $"FileName  : {record.FileName}",
            $"Offset    : {record.Offset:N0}",
            $"Size      : {record.Size:N0}",
            $"CanExtract: {(record.CanExtract ? "YES" : "NO")}",
            $"Format    : {record.Format}");
    }
}
