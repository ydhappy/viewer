namespace Viewer.App.Pak;

public sealed class SpriteResourcePanel : UserControl
{
    private readonly TextBox _filterBox = new();
    private readonly Button _filterButton = new();
    private readonly Button _diagnosticsButton = new();
    private readonly Button _extractButton = new();
    private readonly Button _saveInfoButton = new();
    private readonly Button _saveRawButton = new();
    private readonly Button _savePreviewButton = new();
    private readonly ListView _entryList = new();
    private readonly TextBox _detailBox = new();
    private readonly Panel _renderPlaceholder = new();
    private readonly PictureBox _rawPreviewBox = new();

    private SpriteListCatalog? _catalog;
    private IReadOnlyList<IdxRecord> _records = Array.Empty<IdxRecord>();
    private IReadOnlyList<SpriteListEntry> _visibleEntries = Array.Empty<SpriteListEntry>();
    private string? _idxPath;
    private byte[]? _lastSprBytes;
    private SpriteRawPreviewResult? _lastRawPreview;

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
        _diagnosticsButton.Text = "SPR 진단";
        _diagnosticsButton.AutoSize = true;
        _extractButton.Text = "SPR 추출";
        _extractButton.AutoSize = true;
        _saveInfoButton.Text = "정보 저장";
        _saveInfoButton.AutoSize = true;
        _saveRawButton.Text = "Raw 저장";
        _saveRawButton.AutoSize = true;
        _savePreviewButton.Text = "Preview 저장";
        _savePreviewButton.AutoSize = true;

        toolbar.Controls.Add(new Label { Text = "Sprite 검색", AutoSize = true, Padding = new Padding(0, 6, 4, 0) });
        toolbar.Controls.Add(_filterBox);
        toolbar.Controls.Add(_filterButton);
        toolbar.Controls.Add(_diagnosticsButton);
        toolbar.Controls.Add(_extractButton);
        toolbar.Controls.Add(_saveInfoButton);
        toolbar.Controls.Add(_saveRawButton);
        toolbar.Controls.Add(_savePreviewButton);

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

        _rawPreviewBox.Dock = DockStyle.Fill;
        _rawPreviewBox.BackColor = Color.Black;
        _rawPreviewBox.SizeMode = PictureBoxSizeMode.Zoom;

        rightTabs.TabPages.Add(new TabPage("Detail") { Controls = { _detailBox } });
        rightTabs.TabPages.Add(new TabPage("Render") { Controls = { _renderPlaceholder } });
        rightTabs.TabPages.Add(new TabPage("Raw Preview") { Controls = { _rawPreviewBox } });

        split.Panel1.Controls.Add(_entryList);
        split.Panel2.Controls.Add(rightTabs);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(split, 0, 1);
        Controls.Add(layout);

        _filterButton.Click += (_, _) => ApplyFilter();
        _diagnosticsButton.Click += (_, _) => ShowSprDiagnostics(rightTabs);
        _extractButton.Click += (_, _) => ExtractSelectedSpr();
        _saveInfoButton.Click += (_, _) => SaveCurrentInfo();
        _saveRawButton.Click += (_, _) => SaveCurrentRawBytes();
        _savePreviewButton.Click += (_, _) => SaveCurrentRawPreview();
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

    public void SetRecords(IReadOnlyList<IdxRecord> records, string? idxPath = null)
    {
        _records = records;
        _idxPath = idxPath;
        ShowInitialState();
    }

    private void ApplyFilter()
    {
        ClearRawPreview();
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
            $"IDX Path       : {(_idxPath ?? "-")}" + Environment.NewLine +
            "※ 목록은 UI 부하를 막기 위해 최대 5,000개까지만 표시합니다.";
        _renderPlaceholder.Invalidate();
    }

    private void ShowInitialState()
    {
        ClearRawPreview();
        if (_catalog is null)
        {
            _entryList.Items.Clear();
            _detailBox.Text = string.Join(Environment.NewLine,
                "Sprite Resource",
                "===============",
                $"SPR Records: {CountSprRecords():N0}",
                $"IDX Path   : {(_idxPath ?? "-")}",
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
        ClearRawPreview();
        var entry = GetSelectedEntry();
        if (entry is null)
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
        lines.Add("SPR 진단 버튼으로 매핑된 .spr 바이트 HEX preview와 raw grayscale preview를 확인할 수 있습니다.");

        return string.Join(Environment.NewLine, lines);
    }

    private void ShowSprDiagnostics(TabControl rightTabs)
    {
        ClearRawPreview();
        var selected = GetSelectedEntry();
        if (selected is null)
        {
            _detailBox.Text = "진단할 Sprite entry를 먼저 선택하세요.";
            return;
        }

        var record = FindSprRecord(selected);
        if (record is null)
        {
            _detailBox.Text = BuildEntryDetail(selected, null) + Environment.NewLine + Environment.NewLine + "SPR 진단 실패: 매핑된 .spr 레코드가 없습니다.";
            return;
        }

        if (string.IsNullOrEmpty(_idxPath))
        {
            _detailBox.Text = BuildEntryDetail(selected, record) + Environment.NewLine + Environment.NewLine + "SPR 진단 실패: IDX 경로가 없습니다. PAK 탭에서 IDX를 다시 여세요.";
            return;
        }

        try
        {
            var pakPath = PakExtractor.ResolvePakPath(_idxPath);
            var data = PakExtractor.ReadBytes(pakPath, record);
            var analysis = SpriteHeaderAnalyzer.Analyze(data);
            var preview = SpriteRawPreviewBuilder.Build(data, analysis);
            _lastSprBytes = data;
            _lastRawPreview = preview;
            _rawPreviewBox.Image = new Bitmap(preview.Bitmap);

            _detailBox.Text = BuildEntryDetail(selected, record) + Environment.NewLine + Environment.NewLine +
                "SPR Byte Diagnostics" + Environment.NewLine +
                "====================" + Environment.NewLine +
                $"PAK Path : {pakPath}" + Environment.NewLine +
                $"Bytes    : {data.Length:N0}" + Environment.NewLine +
                $"Signature: {BuildSignature(data)}" + Environment.NewLine +
                string.Empty + Environment.NewLine +
                analysis.ToDisplayText() + Environment.NewLine + Environment.NewLine +
                preview.ToDisplayText() + Environment.NewLine + Environment.NewLine +
                "HEX Preview" + Environment.NewLine +
                "-----------" + Environment.NewLine +
                PreviewHelper.ToHexPreview(data, 1024);

            rightTabs.SelectedIndex = 2;
        }
        catch (Exception ex)
        {
            _detailBox.Text = BuildEntryDetail(selected, record) + Environment.NewLine + Environment.NewLine + "SPR 진단 실패: " + ex.Message;
        }
    }

    private void ExtractSelectedSpr()
    {
        var selected = GetSelectedEntry();
        if (selected is null)
        {
            _detailBox.Text = "추출할 Sprite entry를 먼저 선택하세요.";
            return;
        }

        var record = FindSprRecord(selected);
        if (record is null)
        {
            _detailBox.Text = BuildEntryDetail(selected, null) + Environment.NewLine + Environment.NewLine + "SPR 추출 실패: 매핑된 .spr 레코드가 없습니다.";
            return;
        }

        if (string.IsNullOrEmpty(_idxPath))
        {
            _detailBox.Text = BuildEntryDetail(selected, record) + Environment.NewLine + Environment.NewLine + "SPR 추출 실패: IDX 경로가 없습니다. PAK 탭에서 IDX를 다시 여세요.";
            return;
        }

        using var dialog = new FolderBrowserDialog
        {
            Description = "매핑된 SPR 리소스를 추출할 폴더 선택"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        try
        {
            var pakPath = PakExtractor.ResolvePakPath(_idxPath);
            var outputPath = PakExtractor.Extract(pakPath, record, dialog.SelectedPath);
            _detailBox.Text = BuildEntryDetail(selected, record) + Environment.NewLine + Environment.NewLine + "SPR 추출 완료: " + outputPath;
        }
        catch (Exception ex)
        {
            _detailBox.Text = BuildEntryDetail(selected, record) + Environment.NewLine + Environment.NewLine + "SPR 추출 실패: " + ex.Message;
        }
    }

    private void SaveCurrentInfo()
    {
        using var dialog = new SaveFileDialog
        {
            Title = "Sprite 매핑 정보 저장",
            Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*",
            FileName = "sprite-info.txt"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        File.WriteAllText(dialog.FileName, _detailBox.Text, System.Text.Encoding.UTF8);
        _detailBox.Text += Environment.NewLine + Environment.NewLine + "정보 저장 완료: " + dialog.FileName;
    }

    private void SaveCurrentRawBytes()
    {
        if (_lastSprBytes is null || _lastSprBytes.Length == 0)
        {
            _detailBox.Text += Environment.NewLine + Environment.NewLine + "저장할 SPR raw byte가 없습니다. 먼저 SPR 진단을 실행하세요.";
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "SPR Raw Byte 저장",
            Filter = "SPR files (*.spr)|*.spr|Binary files (*.bin)|*.bin|All files (*.*)|*.*",
            FileName = "sprite-raw.spr"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        File.WriteAllBytes(dialog.FileName, _lastSprBytes);
        _detailBox.Text += Environment.NewLine + Environment.NewLine + "SPR raw byte 저장 완료: " + dialog.FileName;
    }

    private void SaveCurrentRawPreview()
    {
        if (_lastRawPreview is null || _rawPreviewBox.Image is null)
        {
            _detailBox.Text += Environment.NewLine + Environment.NewLine + "저장할 SPR raw preview가 없습니다. 먼저 SPR 진단을 실행하세요.";
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Title = "SPR Raw Preview PNG 저장",
            Filter = "PNG image (*.png)|*.png",
            FileName = "sprite-raw-preview.png"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _rawPreviewBox.Image.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
        _detailBox.Text += Environment.NewLine + Environment.NewLine + "SPR raw preview 저장 완료: " + dialog.FileName;
    }

    private SpriteListEntry? GetSelectedEntry()
    {
        if (_entryList.SelectedItems.Count != 1)
        {
            return null;
        }

        return _entryList.SelectedItems[0].Tag as SpriteListEntry;
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

    private static string BuildSignature(byte[] data)
    {
        if (data.Length == 0)
        {
            return "empty";
        }

        return string.Join(' ', data.Take(Math.Min(16, data.Length)).Select(value => value.ToString("X2")));
    }

    private void ClearRawPreview()
    {
        _lastSprBytes = null;
        _lastRawPreview = null;
        var oldImage = _rawPreviewBox.Image;
        _rawPreviewBox.Image = null;
        oldImage?.Dispose();
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
        graphics.DrawString("SPR 진단/추출/정보 저장/raw 저장/preview 저장 기능도 연결되었습니다.", font, brush, 44, 118);
        graphics.DrawString("Raw Preview 탭에서 후보 payload 회색조 preview를 확인할 수 있습니다.", font, brush, 44, 144);
        graphics.DrawString("실제 프레임 디코딩/팔레트/방향별 렌더링은 다음 단계에서 연결합니다.", font, brush, 44, 170);
    }
}
