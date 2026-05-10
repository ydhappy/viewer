using System.Text;

namespace Viewer.App;

public sealed class MainForm : Form
{
    private readonly TabControl _tabs = new();
    private readonly TextBox _logBox = new();
    private readonly Pak.SpriteResourcePanel _spritePanel = new();

    private string? _currentIdxPath;
    private List<Pak.IdxRecord> _currentPakRecords = new();
    private Pak.SpriteListCatalog? _spriteListCatalog;

    public MainForm()
    {
        Text = "viewer - Pak / Map 통합 뷰어";
        Width = 1280;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;
        _tabs.Dock = DockStyle.Fill;
        _tabs.TabPages.Add(CreatePakPage());
        _tabs.TabPages.Add(CreateSpritePage());
        _tabs.TabPages.Add(CreateMapPage());
        _tabs.TabPages.Add(CreateLogPage());
        Controls.Add(_tabs);
        WriteLog("viewer started");
    }

    private TabPage CreatePakPage()
    {
        var page = new TabPage("PAK / IDX");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8) };
        var openIdxButton = new Button { Text = "IDX 열기", AutoSize = true };
        var openSpriteListButton = new Button { Text = "list.spr 열기", AutoSize = true };
        var exportButton = new Button { Text = "선택 추출", AutoSize = true, Enabled = false };
        toolbar.Controls.Add(openIdxButton);
        toolbar.Controls.Add(openSpriteListButton);
        toolbar.Controls.Add(exportButton);

        var list = CreatePakListView();
        var previewTabs = new TabControl { Dock = DockStyle.Fill };
        var textPreview = CreateReadOnlyTextBox();
        var specialPreview = CreateReadOnlyTextBox();
        var infoPreview = CreateReadOnlyTextBox();
        var imagePreview = new PictureBox { Dock = DockStyle.Fill, SizeMode = PictureBoxSizeMode.Zoom, BackColor = Color.Black };
        previewTabs.TabPages.Add(new TabPage("Text / Hex") { Controls = { textPreview } });
        previewTabs.TabPages.Add(new TabPage("Image") { Controls = { imagePreview } });
        previewTabs.TabPages.Add(new TabPage("Special") { Controls = { specialPreview } });
        previewTabs.TabPages.Add(new TabPage("Info") { Controls = { infoPreview } });

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 680 };
        split.Panel1.Controls.Add(list);
        split.Panel2.Controls.Add(previewTabs);

        list.SelectedIndexChanged += (_, _) =>
        {
            exportButton.Enabled = list.SelectedItems.Cast<ListViewItem>().Any(item => item.Tag is Pak.IdxRecord { CanExtract: true }) && !string.IsNullOrEmpty(_currentIdxPath);
            if (list.SelectedItems.Count == 1 && list.SelectedItems[0].Tag is Pak.IdxRecord record)
            {
                ShowPakPreview(record, previewTabs, textPreview, imagePreview, specialPreview, infoPreview);
            }
            else if (list.SelectedItems.Count > 1)
            {
                ClearImage(imagePreview);
                textPreview.Text = "여러 항목이 선택되었습니다. 미리보기는 단일 선택에서만 표시됩니다.";
                specialPreview.Clear();
                infoPreview.Text = $"Selected: {list.SelectedItems.Count:N0}";
                previewTabs.SelectedIndex = 3;
            }
        };

        openIdxButton.Click += (_, _) => LoadIdx(list, previewTabs, textPreview, imagePreview, specialPreview, infoPreview, exportButton);
        openSpriteListButton.Click += (_, _) => LoadSpriteList(previewTabs, specialPreview);
        exportButton.Click += (_, _) => ExportSelectedPakRecords(list);

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(split, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private static ListView CreatePakListView()
    {
        var list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = true };
        list.Columns.Add("No", 70, HorizontalAlignment.Right);
        list.Columns.Add("FileName", 300, HorizontalAlignment.Left);
        list.Columns.Add("Size", 120, HorizontalAlignment.Right);
        list.Columns.Add("Offset", 120, HorizontalAlignment.Right);
        list.Columns.Add("Extract", 80, HorizontalAlignment.Center);
        list.Columns.Add("Format", 110, HorizontalAlignment.Left);
        return list;
    }

    private static TextBox CreateReadOnlyTextBox()
    {
        return new TextBox { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both, Font = new Font(FontFamily.GenericMonospace, 10) };
    }

    private void LoadIdx(ListView list, TabControl previewTabs, TextBox textPreview, PictureBox imagePreview, TextBox specialPreview, TextBox infoPreview, Button exportButton)
    {
        using var dialog = new OpenFileDialog { Title = "IDX 파일 선택", Filter = "IDX files (*.idx)|*.idx|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            ClearImage(imagePreview);
            textPreview.Clear();
            specialPreview.Clear();
            infoPreview.Clear();

            var state = Pak.IdxLoadUiBinder.Load(dialog.FileName);
            _currentIdxPath = dialog.FileName;
            _currentPakRecords = state.Records.ToList();
            Pak.IdxLoadUiBinder.FillListView(list, state.Records);
            _spritePanel.SetRecords(_currentPakRecords, _currentIdxPath);
            exportButton.Enabled = false;
            infoPreview.Text = state.InfoText;
            previewTabs.SelectedIndex = 3;
            WriteLog(state.LogText);

            if (!string.IsNullOrWhiteSpace(state.ProbeWarning))
            {
                MessageBox.Show(this, state.ProbeWarning, "IDX parser probe-only", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "IDX load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            WriteLog("IDX load failed: " + ex.Message);
        }
    }

    private void LoadSpriteList(TabControl previewTabs, TextBox specialPreview)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "list.spr 파일 선택",
            Filter = "SPR list files (list.spr;*.spr;*.txt;*.csv)|list.spr;*.spr;*.txt;*.csv|All files (*.*)|*.*"
        };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        try
        {
            _spriteListCatalog = Pak.SpriteListParser.Parse(dialog.FileName);
            _spritePanel.SetCatalog(_spriteListCatalog);
            specialPreview.Text = _spriteListCatalog.ToDisplayText();
            previewTabs.SelectedIndex = 2;
            WriteLog($"list.spr loaded: {dialog.FileName}, entries={_spriteListCatalog.Entries.Count}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "list.spr load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            WriteLog("list.spr load failed: " + ex.Message);
        }
    }

    private void ExportSelectedPakRecords(ListView list)
    {
        if (string.IsNullOrEmpty(_currentIdxPath) || list.SelectedItems.Count == 0) return;
        using var dialog = new FolderBrowserDialog { Description = "추출할 폴더 선택" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;

        var pakPath = Pak.PakExtractor.ResolvePakPath(_currentIdxPath);
        var success = 0;
        var failed = 0;
        var skipped = 0;
        foreach (ListViewItem item in list.SelectedItems)
        {
            if (item.Tag is not Pak.IdxRecord record)
            {
                failed++;
                continue;
            }

            if (!record.CanExtract)
            {
                skipped++;
                WriteLog($"Extract skipped: {record.FileName} - not extractable");
                continue;
            }

            try
            {
                var outputPath = Pak.PakExtractor.Extract(pakPath, record, dialog.SelectedPath);
                success++;
                WriteLog("Extracted: " + outputPath);
            }
            catch (Exception ex)
            {
                failed++;
                WriteLog($"Extract failed: {record.FileName} - {ex.Message}");
            }
        }

        MessageBox.Show(this, $"추출 완료\n성공: {success}\n실패: {failed}\n건너뜀: {skipped}", "Extract", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private TabPage CreateSpritePage()
    {
        var page = new TabPage("Sprite");
        page.Controls.Add(_spritePanel);
        return page;
    }

    private void ShowPakPreview(Pak.IdxRecord record, TabControl tabs, TextBox textPreview, PictureBox imagePreview, TextBox specialPreview, TextBox infoPreview)
    {
        ClearImage(imagePreview);
        textPreview.Clear();
        specialPreview.Clear();
        infoPreview.Text = string.Join(Environment.NewLine,
            $"FileName : {record.FileName}",
            $"Size     : {record.Size:N0}",
            $"Offset   : {record.Offset:N0}",
            $"Format   : {record.Format}",
            $"Extract  : {(record.CanExtract ? "YES" : "NO")}");

        if (string.IsNullOrEmpty(_currentIdxPath) || !record.CanExtract)
        {
            textPreview.Text = "추출/미리보기 가능한 레코드가 아닙니다.";
            tabs.SelectedIndex = 3;
            return;
        }

        try
        {
            var pakPath = Pak.PakExtractor.ResolvePakPath(_currentIdxPath);
            var data = Pak.PakExtractor.ReadBytes(pakPath, record);
            var kind = Pak.PreviewHelper.DetectKind(record.FileName, data);
            infoPreview.AppendText(Environment.NewLine + $"Preview  : {kind}" + Environment.NewLine + $"PakPath  : {pakPath}");

            switch (kind)
            {
                case Pak.PreviewKind.Text:
                    textPreview.Text = Pak.PreviewHelper.DecodeText(data);
                    tabs.SelectedIndex = 0;
                    break;
                case Pak.PreviewKind.Image:
                    imagePreview.Image = Pak.PreviewHelper.LoadImage(data);
                    tabs.SelectedIndex = 1;
                    break;
                case Pak.PreviewKind.Special:
                    specialPreview.Text = BuildSpecialPreviewText(record, data);
                    tabs.SelectedIndex = 2;
                    break;
                case Pak.PreviewKind.Hex:
                    textPreview.Text = Pak.PreviewHelper.ToHexPreview(data);
                    tabs.SelectedIndex = 0;
                    break;
                default:
                    textPreview.Text = "지원하지 않는 미리보기 형식입니다. 추출 후 외부 도구로 확인하세요.";
                    tabs.SelectedIndex = 3;
                    break;
            }
        }
        catch (Exception ex)
        {
            textPreview.Text = ex.Message;
            infoPreview.AppendText(Environment.NewLine + "Preview error: " + ex.Message);
            tabs.SelectedIndex = 3;
        }
    }

    private string BuildSpecialPreviewText(Pak.IdxRecord record, byte[] data)
    {
        var builder = new StringBuilder();
        builder.AppendLine(Pak.SpecialResourceAnalyzer.Analyze(record.FileName, data).ToDisplayText());
        if (Path.GetExtension(record.FileName).Equals(".spr", StringComparison.OrdinalIgnoreCase))
        {
            builder.AppendLine();
            builder.AppendLine("Sprite Mapping");
            builder.AppendLine("==============");
            var mapped = _spriteListCatalog is null ? null : FindSpriteMapping(record);
            builder.AppendLine(mapped is null ? "No matching list.spr entry found or list.spr is not loaded." : mapped.ToDisplayText());
        }
        return builder.ToString();
    }

    private Pak.SpriteListEntry? FindSpriteMapping(Pak.IdxRecord record)
    {
        if (_spriteListCatalog is null) return null;
        var byIndex = _spriteListCatalog.FindBySpriteId(record.Index);
        if (byIndex is not null) return byIndex;
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(record.FileName);
        if (int.TryParse(nameWithoutExtension, out var spriteId))
        {
            var byFileNumber = _spriteListCatalog.FindBySpriteId(spriteId);
            if (byFileNumber is not null) return byFileNumber;
        }
        return _spriteListCatalog.FindByName(nameWithoutExtension);
    }

    private static void ClearImage(PictureBox pictureBox)
    {
        var oldImage = pictureBox.Image;
        pictureBox.Image = null;
        oldImage?.Dispose();
    }

    private TabPage CreateMapPage()
    {
        var currentMapInfos = new List<Map.S32Info>();
        var page = new TabPage("S32 Map");
        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.LeftToRight, Padding = new Padding(8) };
        var openMapButton = new Button { Text = "S32 열기", AutoSize = true };
        var openFolderButton = new Button { Text = "지도 폴더 스캔", AutoSize = true };
        var openTileButton = new Button { Text = "Tile.idx 열기", AutoSize = true };
        var zoomInButton = new Button { Text = "확대", AutoSize = true };
        var zoomOutButton = new Button { Text = "축소", AutoSize = true };
        var resetZoomButton = new Button { Text = "100%", AutoSize = true };
        var saveRenderButton = new Button { Text = "PNG 저장", AutoSize = true };
        var copyTileButton = new Button { Text = "Tile 복사", AutoSize = true };
        var saveInfoButton = new Button { Text = "Info 저장", AutoSize = true };
        var saveCsvButton = new Button { Text = "CSV 저장", AutoSize = true };
        var tileStatusLabel = new Label { Text = "Tile: not loaded", AutoSize = true, Padding = new Padding(12, 8, 0, 0) };
        toolbar.Controls.AddRange(new Control[] { openMapButton, openFolderButton, openTileButton, zoomInButton, zoomOutButton, resetZoomButton, saveRenderButton, copyTileButton, saveInfoButton, saveCsvButton, tileStatusLabel });

        var mapList = CreateMapListView();
        var mapInfoBox = CreateReadOnlyTextBox();
        var tilePanel = new Map.TileResourcePanel();
        var mapRenderPanel = new Map.S32GridRenderPanel { Dock = DockStyle.Fill };
        var mapPreviewTabs = new TabControl { Dock = DockStyle.Fill };
        mapPreviewTabs.TabPages.Add(new TabPage("Info") { Controls = { mapInfoBox } });
        mapPreviewTabs.TabPages.Add(new TabPage("Render") { Controls = { mapRenderPanel } });
        mapPreviewTabs.TabPages.Add(new TabPage("Tile") { Controls = { tilePanel } });

        var split = new SplitContainer { Dock = DockStyle.Fill, Orientation = Orientation.Vertical, SplitterDistance = 680 };
        split.Panel1.Controls.Add(mapList);
        split.Panel2.Controls.Add(mapPreviewTabs);

        zoomInButton.Click += (_, _) => { mapRenderPanel.ZoomIn(); mapPreviewTabs.SelectedIndex = 1; };
        zoomOutButton.Click += (_, _) => { mapRenderPanel.ZoomOut(); mapPreviewTabs.SelectedIndex = 1; };
        resetZoomButton.Click += (_, _) => { mapRenderPanel.ResetZoom(); mapPreviewTabs.SelectedIndex = 1; };
        copyTileButton.Click += (_, _) => { Clipboard.SetText(mapRenderPanel.GetSelectedTileInfoText()); WriteLog("Tile info copied to clipboard"); };
        saveRenderButton.Click += (_, _) => SaveMapRender(mapRenderPanel);
        saveInfoButton.Click += (_, _) => SaveTextFromBox(mapInfoBox, "S32 분석 정보 저장", "s32-info.txt", "S32 info saved: ");
        saveCsvButton.Click += (_, _) => SaveS32Csv(currentMapInfos);
        openMapButton.Click += (_, _) => OpenS32File(currentMapInfos, mapList, mapInfoBox, mapRenderPanel, mapPreviewTabs);
        openFolderButton.Click += (_, _) => ScanS32Folder(currentMapInfos, mapList, mapInfoBox, mapRenderPanel, mapPreviewTabs);
        openTileButton.Click += (_, _) => OpenTileIdx(tilePanel, mapRenderPanel, mapPreviewTabs, tileStatusLabel);
        mapList.SelectedIndexChanged += (_, _) =>
        {
            if (mapList.SelectedItems.Count == 1 && mapList.SelectedItems[0].Tag is Map.S32Info info)
            {
                mapInfoBox.Text = info.ToDisplayText();
                mapRenderPanel.SetMap(info);
                mapPreviewTabs.SelectedIndex = 0;
            }
        };

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(split, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private static ListView CreateMapListView()
    {
        var list = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, MultiSelect = false };
        list.Columns.Add("FileName", 220, HorizontalAlignment.Left);
        list.Columns.Add("X", 80, HorizontalAlignment.Right);
        list.Columns.Add("Y", 80, HorizontalAlignment.Right);
        list.Columns.Add("Size", 120, HorizontalAlignment.Right);
        list.Columns.Add("Candidate", 320, HorizontalAlignment.Left);
        return list;
    }

    private void OpenS32File(List<Map.S32Info> currentMapInfos, ListView mapList, TextBox mapInfoBox, Map.S32GridRenderPanel mapRenderPanel, TabControl mapPreviewTabs)
    {
        using var dialog = new OpenFileDialog { Title = "S32 파일 선택", Filter = "S32 files (*.s32)|*.s32|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        var info = Map.S32Analyzer.Analyze(dialog.FileName);
        currentMapInfos.Clear();
        currentMapInfos.Add(info);
        mapList.Items.Clear();
        AddS32Item(mapList, info);
        mapInfoBox.Text = info.ToDisplayText();
        mapRenderPanel.SetMap(info);
        mapPreviewTabs.SelectedIndex = 1;
        WriteLog("S32 analyzed: " + dialog.FileName);
    }

    private void ScanS32Folder(List<Map.S32Info> currentMapInfos, ListView mapList, TextBox mapInfoBox, Map.S32GridRenderPanel mapRenderPanel, TabControl mapPreviewTabs)
    {
        using var dialog = new FolderBrowserDialog { Description = "S32 지도 폴더 선택" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var infos = Map.S32Analyzer.ScanFolder(dialog.SelectedPath);
            currentMapInfos.Clear();
            currentMapInfos.AddRange(infos);
            mapList.Items.Clear();
            foreach (var info in infos) AddS32Item(mapList, info);
            mapInfoBox.Text = $"Folder: {dialog.SelectedPath}{Environment.NewLine}S32 files: {infos.Count:N0}";
            mapRenderPanel.SetMap(infos.FirstOrDefault());
            mapPreviewTabs.SelectedIndex = infos.Count > 0 ? 1 : 0;
            WriteLog($"Map folder scanned: {dialog.SelectedPath}, s32={infos.Count}");
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "S32 scan failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            WriteLog("S32 scan failed: " + ex.Message);
        }
    }

    private void OpenTileIdx(Map.TileResourcePanel tilePanel, Map.S32GridRenderPanel mapRenderPanel, TabControl mapPreviewTabs, Label tileStatusLabel)
    {
        using var dialog = new OpenFileDialog { Title = "Tile.idx 파일 선택", Filter = "IDX files (*.idx)|*.idx|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var tileResourceSet = Map.TileResourceSet.Load(dialog.FileName);
            mapRenderPanel.SetTileResource(tileResourceSet);
            tilePanel.SetTileResource(tileResourceSet);
            tileStatusLabel.Text = $"Tile: {tileResourceSet.ExtractableRecords:N0}/{tileResourceSet.TotalRecords:N0}";
            mapPreviewTabs.SelectedIndex = 2;
            WriteLog("Tile.idx loaded: " + dialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Tile.idx load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
            WriteLog("Tile.idx load failed: " + ex.Message);
        }
    }

    private void SaveMapRender(Map.S32GridRenderPanel mapRenderPanel)
    {
        using var dialog = new SaveFileDialog { Title = "Render PNG 저장", Filter = "PNG image (*.png)|*.png", FileName = "s32-render.png" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        using var bitmap = mapRenderPanel.CreateSnapshot();
        bitmap.Save(dialog.FileName, System.Drawing.Imaging.ImageFormat.Png);
        WriteLog("Render PNG saved: " + dialog.FileName);
    }

    private void SaveTextFromBox(TextBox box, string title, string fileName, string logPrefix)
    {
        using var dialog = new SaveFileDialog { Title = title, Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*", FileName = fileName };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllText(dialog.FileName, box.Text, Encoding.UTF8);
        WriteLog(logPrefix + dialog.FileName);
    }

    private void SaveS32Csv(List<Map.S32Info> currentMapInfos)
    {
        if (currentMapInfos.Count == 0)
        {
            MessageBox.Show(this, "저장할 S32 스캔 결과가 없습니다.", "CSV 저장", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        using var dialog = new SaveFileDialog { Title = "S32 스캔 결과 CSV 저장", Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*", FileName = "s32-scan.csv" };
        if (dialog.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllText(dialog.FileName, BuildS32Csv(currentMapInfos), Encoding.UTF8);
        WriteLog("S32 CSV saved: " + dialog.FileName);
    }

    private static void AddS32Item(ListView list, Map.S32Info info)
    {
        var item = new ListViewItem(info.FileName) { Tag = info };
        item.SubItems.Add(info.Coordinate?.X.ToString() ?? "");
        item.SubItems.Add(info.Coordinate?.Y.ToString() ?? "");
        item.SubItems.Add(info.FileSize.ToString("N0"));
        item.SubItems.Add(info.LayerCandidateSummary);
        list.Items.Add(item);
    }

    private static string BuildS32Csv(IEnumerable<Map.S32Info> infos)
    {
        var builder = new StringBuilder();
        builder.AppendLine("FileName,X,Y,Size,CoordinateSource,Candidate,Path");
        foreach (var info in infos)
        {
            builder.Append(CsvEscape(info.FileName)).Append(',');
            builder.Append(info.Coordinate?.X.ToString() ?? string.Empty).Append(',');
            builder.Append(info.Coordinate?.Y.ToString() ?? string.Empty).Append(',');
            builder.Append(info.FileSize).Append(',');
            builder.Append(CsvEscape(info.Coordinate?.Source ?? string.Empty)).Append(',');
            builder.Append(CsvEscape(info.LayerCandidateSummary)).Append(',');
            builder.AppendLine(CsvEscape(info.FilePath));
        }
        return builder.ToString();
    }

    private static string CsvEscape(string value)
    {
        return value.Contains(',') || value.Contains('"') || value.Contains('\r') || value.Contains('\n')
            ? '"' + value.Replace("\"", "\"\"") + '"'
            : value;
    }

    private TabPage CreateLogPage()
    {
        var page = new TabPage("Log");
        _logBox.Dock = DockStyle.Fill;
        _logBox.Multiline = true;
        _logBox.ReadOnly = true;
        _logBox.ScrollBars = ScrollBars.Both;
        _logBox.Font = new Font(FontFamily.GenericMonospace, 10);
        page.Controls.Add(_logBox);
        return page;
    }

    private void WriteLog(string message)
    {
        _logBox.AppendText($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}{Environment.NewLine}");
    }
}
