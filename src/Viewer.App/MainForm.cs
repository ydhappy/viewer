namespace Viewer.App;

public sealed class MainForm : Form
{
    private readonly TabControl _tabs = new();
    private readonly TextBox _logBox = new();

    public MainForm()
    {
        Text = "viewer - Pak / Map 통합 뷰어";
        Width = 1280;
        Height = 800;
        StartPosition = FormStartPosition.CenterScreen;

        _tabs.Dock = DockStyle.Fill;
        _tabs.TabPages.Add(CreatePakPage());
        _tabs.TabPages.Add(CreateMapPage());
        _tabs.TabPages.Add(CreateLogPage());

        Controls.Add(_tabs);
        WriteLog("viewer started");
    }

    private TabPage CreatePakPage()
    {
        var page = new TabPage("PAK / IDX");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(8)
        };

        var openIdxButton = new Button { Text = "IDX 열기", AutoSize = true };
        var exportButton = new Button { Text = "선택 추출", AutoSize = true, Enabled = false };
        toolbar.Controls.Add(openIdxButton);
        toolbar.Controls.Add(exportButton);

        var list = new ListView
        {
            Dock = DockStyle.Fill,
            View = View.Details,
            FullRowSelect = true,
            GridLines = true
        };
        list.Columns.Add("No", 70, HorizontalAlignment.Right);
        list.Columns.Add("FileName", 300, HorizontalAlignment.Left);
        list.Columns.Add("Size", 120, HorizontalAlignment.Right);
        list.Columns.Add("Offset", 120, HorizontalAlignment.Right);

        openIdxButton.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Title = "IDX 파일 선택",
                Filter = "IDX files (*.idx)|*.idx|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            try
            {
                list.Items.Clear();
                var records = Pak.IdxParser.Parse(dialog.FileName);
                foreach (var record in records)
                {
                    var item = new ListViewItem(record.Index.ToString());
                    item.SubItems.Add(record.FileName);
                    item.SubItems.Add(record.Size.ToString("N0"));
                    item.SubItems.Add(record.Offset.ToString("N0"));
                    list.Items.Add(item);
                }
                WriteLog($"IDX loaded: {dialog.FileName}, records={records.Count}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "IDX load failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                WriteLog("IDX load failed: " + ex.Message);
            }
        };

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(list, 0, 1);
        page.Controls.Add(layout);
        return page;
    }

    private TabPage CreateMapPage()
    {
        var page = new TabPage("S32 Map");
        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 2
        };
        layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 44));
        layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        var toolbar = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            Padding = new Padding(8)
        };

        var openMapButton = new Button { Text = "S32 열기", AutoSize = true };
        var openFolderButton = new Button { Text = "지도 폴더 스캔", AutoSize = true };
        toolbar.Controls.Add(openMapButton);
        toolbar.Controls.Add(openFolderButton);

        var infoBox = new TextBox
        {
            Dock = DockStyle.Fill,
            Multiline = true,
            ReadOnly = true,
            ScrollBars = ScrollBars.Both,
            Font = new Font(FontFamily.GenericMonospace, 10)
        };

        openMapButton.Click += (_, _) =>
        {
            using var dialog = new OpenFileDialog
            {
                Title = "S32 파일 선택",
                Filter = "S32 files (*.s32)|*.s32|All files (*.*)|*.*"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var info = Map.S32Analyzer.Analyze(dialog.FileName);
            infoBox.Text = info.ToDisplayText();
            WriteLog("S32 analyzed: " + dialog.FileName);
        };

        openFolderButton.Click += (_, _) =>
        {
            using var dialog = new FolderBrowserDialog
            {
                Description = "S32 지도 폴더 선택"
            };

            if (dialog.ShowDialog(this) != DialogResult.OK)
            {
                return;
            }

            var files = Directory.GetFiles(dialog.SelectedPath, "*.s32", SearchOption.TopDirectoryOnly);
            infoBox.Text = string.Join(Environment.NewLine, files.Select(Path.GetFileName));
            WriteLog($"Map folder scanned: {dialog.SelectedPath}, s32={files.Length}");
        };

        layout.Controls.Add(toolbar, 0, 0);
        layout.Controls.Add(infoBox, 0, 1);
        page.Controls.Add(layout);
        return page;
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
