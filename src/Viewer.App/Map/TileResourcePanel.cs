namespace Viewer.App.Map;

public sealed class TileResourcePanel : UserControl
{
    private readonly TextBox _searchBox = new();
    private readonly Button _searchButton = new();
    private readonly Button _diagnosticsButton = new();
    private readonly Button _converterButton = new();
    private readonly Button _saveImageButton = new();
    private readonly Button _copyImageButton = new();
    private readonly ListView _recordList = new();
    private readonly TextBox _detailBox = new();
    private readonly PictureBox _imagePreview = new();
    private readonly DefaultTileImageCache _imageCache = new();

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
        _searchButton.Text = "검색/변환";
        _searchButton.AutoSize = true;
        _diagnosticsButton.Text = "진단";
        _diagnosticsButton.AutoSize = true;
        _converterButton.Text = "변환기 목록";
        _converterButton.AutoSize = true;
        _saveImageButton.Text = "이미지 저장";
        _saveImageButton.AutoSize = true;
        _copyImageButton.Text = "이미지 복사";
        _copyImageButton.AutoSize = true;
        toolbar.Controls.Add(new Label { Text = "Tile ID", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        toolbar.Controls.Add(_searchBox);
        toolbar.Controls.Add(_searchButton);
        toolbar.Controls.Add(_diagnosticsButton);
        toolbar.Controls.Add(_converterButton);
        toolbar.Controls.Add(_saveImageButton);
        toolbar.Controls.Add(_copyImageButton);

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
        _recordList.Columns.Add("Kind", 90, HorizontalAlignment.Left);
        _recordList.Columns.Add("Size", 110, HorizontalAlignment.Right);
        _recordList.Columns.Add("Offset", 110, HorizontalAlignment.Right);
        _recordList.Columns.Add("Extract", 70, HorizontalAlignment.Center);

        var rightTabs = new TabControl { Dock = DockStyle.Fill };
        _detailBox.Dock = DockStyle.Fill;
        _detailBox.Multiline = true;
        _detailBox.ReadOnly = true;
        _detailBox.ScrollBars = ScrollBars.Both;
        _detailBox.Font = new Font(FontFamily.GenericMonospace, 10);
        _detailBox.Text = "Tile.idx를 열면 이곳에 타일 리소스 상태와 레코드 목록이 표시됩니다.";

        _imagePreview.Dock = DockStyle.Fill;
        _imagePreview.SizeMode = PictureBoxSizeMode.Zoom;
        _imagePreview.BackColor = Color.Black;

        rightTabs.TabPages.Add(new TabPage("Detail") { Controls = { _detailBox } });
        rightTabs.TabPages.Add(new TabPage("Image") { Controls = { _imagePreview } });

        split.Panel1.Controls.Add(_recordList);
        split.Panel2.Controls.Add(rightTabs);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(split, 0, 1);
        Controls.Add(layout);

        _searchButton.Click += (_, _) => SearchTile(rightTabs);
        _diagnosticsButton.Click += (_, _) => ShowDiagnostics(rightTabs);
        _converterButton.Click += (_, _) =>
        {
            ClearImage();
            _detailBox.Text = _imageCache.GetConverterListText();
            rightTabs.SelectedIndex = 0;
        };
        _saveImageButton.Click += (_, _) => SaveCurrentImage();
        _copyImageButton.Click += (_, _) => CopyCurrentImage();
        _searchBox.KeyDown += (_, e) =>
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                SearchTile(rightTabs);
            }
        };
        _recordList.SelectedIndexChanged += (_, _) => ShowSelectedRecord();
    }

    public void SetTileResource(TileResourceSet tileResourceSet)
    {
        _tileResourceSet = tileResourceSet;
        _recordList.Items.Clear();
        ClearImage();

        foreach (var record in tileResourceSet.Records.Take(5000))
        {
            var candidate = TileResourceClassifier.Classify(record);
            var item = new ListViewItem(record.Index.ToString())
            {
                Tag = record
            };
            item.SubItems.Add(record.FileName);
            item.SubItems.Add(candidate.Kind.ToString());
            item.SubItems.Add(record.Size.ToString("N0"));
            item.SubItems.Add(record.Offset.ToString("N0"));
            item.SubItems.Add(record.CanExtract ? "YES" : "NO");
            _recordList.Items.Add(item);
        }

        _detailBox.Text = tileResourceSet.ToDisplayText() + Environment.NewLine + Environment.NewLine +
            $"Displayed Records: {_recordList.Items.Count:N0}" + Environment.NewLine +
            "※ 목록은 과도한 UI 부하를 막기 위해 최대 5,000개까지만 표시합니다." + Environment.NewLine + Environment.NewLine +
            _imageCache.GetConverterListText();
    }

    private void SearchTile(TabControl rightTabs)
    {
        ClearImage();

        if (_tileResourceSet is null)
        {
            _detailBox.Text = "Tile.idx가 아직 로드되지 않았습니다.";
            rightTabs.SelectedIndex = 0;
            return;
        }

        if (!int.TryParse(_searchBox.Text.Trim(), out var tileId))
        {
            _detailBox.Text = "검색할 Tile ID를 숫자로 입력하세요.";
            rightTabs.SelectedIndex = 0;
            return;
        }

        var conversionResult = _imageCache.GetTileImage(tileId, _tileResourceSet);
        var lookup = new TileRecordLookup(tileId, conversionResult);
        _detailBox.Text = lookup.ToDisplayText();

        if (conversionResult.Record is not null)
        {
            SelectRecord(conversionResult.Record);
        }

        if (conversionResult.Success && conversionResult.Image is not null)
        {
            _imagePreview.Image = new Bitmap(conversionResult.Image);
            rightTabs.SelectedIndex = 1;
        }
        else
        {
            rightTabs.SelectedIndex = 0;
        }
    }

    private void ShowDiagnostics(TabControl rightTabs)
    {
        ClearImage();

        if (_tileResourceSet is null)
        {
            _detailBox.Text = "Tile.idx가 아직 로드되지 않았습니다.";
            rightTabs.SelectedIndex = 0;
            return;
        }

        var record = GetSelectedRecord();
        if (record is null)
        {
            _detailBox.Text = "진단할 Tile 레코드를 목록에서 선택하세요.";
            rightTabs.SelectedIndex = 0;
            return;
        }

        var diagnostics = TileResourceDiagnosticsAnalyzer.Analyze(_tileResourceSet, record);
        _detailBox.Text = diagnostics.ToDisplayText();
        rightTabs.SelectedIndex = 0;
    }

    private void SaveCurrentImage()
    {
        if (_imagePreview.Image is null)
        {
            _detailBox.Text = "저장할 변환 이미지가 없습니다. 먼저 Tile ID 검색/변환을 실행하세요.";
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "Tile 변환 이미지 저장",
            Filter = "PNG image (*.png)|*.png",
            FileName = "tile-converted.png"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _imagePreview.Image.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
        _detailBox.Text += Environment.NewLine + Environment.NewLine + "이미지 저장 완료: " + dialog.FileName;
    }

    private void CopyCurrentImage()
    {
        if (_imagePreview.Image is null)
        {
            _detailBox.Text = "복사할 변환 이미지가 없습니다. 먼저 Tile ID 검색/변환을 실행하세요.";
            return;
        }

        Clipboard.SetImage(new Bitmap(_imagePreview.Image));
        _detailBox.Text += Environment.NewLine + Environment.NewLine + "이미지를 클립보드에 복사했습니다.";
    }

    private Viewer.App.Pak.IdxRecord? GetSelectedRecord()
    {
        if (_recordList.SelectedItems.Count != 1)
        {
            return null;
        }

        return _recordList.SelectedItems[0].Tag as Viewer.App.Pak.IdxRecord;
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
        ClearImage();

        if (_recordList.SelectedItems.Count != 1 || _recordList.SelectedItems[0].Tag is not Viewer.App.Pak.IdxRecord record)
        {
            return;
        }

        var candidate = TileResourceClassifier.Classify(record);
        var converterName = _imageCache.GetConverterName(candidate, record);
        _detailBox.Text = string.Join(Environment.NewLine,
            "Tile Record",
            "===========",
            $"Index     : {record.Index}",
            $"FileName  : {record.FileName}",
            $"Kind      : {candidate.Kind}",
            $"Candidate : {candidate.Description}",
            $"Converter : {converterName}",
            $"Offset    : {record.Offset:N0}",
            $"Size      : {record.Size:N0}",
            $"CanExtract: {(record.CanExtract ? "YES" : "NO")}",
            $"Format    : {record.Format}",
            string.Empty,
            "※ 이미지 변환은 Tile ID 검색/변환 버튼으로 실행합니다.",
            "※ 헤더/HEX 확인은 진단 버튼을 사용하세요.");
    }

    private void ClearImage()
    {
        var oldImage = _imagePreview.Image;
        _imagePreview.Image = null;
        oldImage?.Dispose();
    }
}
