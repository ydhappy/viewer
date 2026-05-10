namespace Viewer.App.Map;

public sealed class S32GridRenderPanel : Panel
{
    private S32Info? _currentMap;
    private TileResourceSet? _tileResourceSet;

    public S32GridRenderPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(24, 24, 24);
    }

    public void SetMap(S32Info? mapInfo)
    {
        _currentMap = mapInfo;
        Invalidate();
    }

    public void SetTileResource(TileResourceSet? tileResourceSet)
    {
        _tileResourceSet = tileResourceSet;
        Invalidate();
    }

    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);

        e.Graphics.Clear(BackColor);
        e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

        if (_currentMap is null)
        {
            DrawCenteredText(e.Graphics, "S32 파일을 선택하세요.");
            return;
        }

        DrawIsoGrid(e.Graphics);
        DrawOverlay(e.Graphics);
    }

    private void DrawIsoGrid(Graphics graphics)
    {
        var centerX = Width / 2;
        var startY = 60;
        const int tileW = 24;
        const int tileH = 12;
        const int rows = 24;
        const int cols = 24;

        using var gridPen = new Pen(Color.FromArgb(70, 120, 120, 120));
        using var highlightPen = new Pen(Color.FromArgb(150, 180, 180, 180));

        for (var y = 0; y < rows; y++)
        {
            for (var x = 0; x < cols; x++)
            {
                var screenX = centerX + (x - y) * tileW / 2;
                var screenY = startY + (x + y) * tileH / 2;

                var diamond = new[]
                {
                    new Point(screenX, screenY),
                    new Point(screenX + tileW / 2, screenY + tileH / 2),
                    new Point(screenX, screenY + tileH),
                    new Point(screenX - tileW / 2, screenY + tileH / 2)
                };

                graphics.DrawPolygon((x + y) % 8 == 0 ? highlightPen : gridPen, diamond);
            }
        }
    }

    private void DrawOverlay(Graphics graphics)
    {
        using var brush = new SolidBrush(Color.White);
        using var dimBrush = new SolidBrush(Color.FromArgb(210, 210, 210));
        using var font = new Font(FontFamily.GenericSansSerif, 10, FontStyle.Regular);
        using var titleFont = new Font(FontFamily.GenericSansSerif, 11, FontStyle.Bold);

        var lines = new List<string>
        {
            $"S32: {_currentMap!.FileName}",
            $"Coord: {(_currentMap.Coordinate is null ? "unknown" : _currentMap.Coordinate.ToString())}",
            $"Size: {_currentMap.FileSize:N0} bytes",
            $"Tile.idx: {(_tileResourceSet is null ? "not loaded" : _tileResourceSet.ExtractableRecords + "/" + _tileResourceSet.TotalRecords)}"
        };

        graphics.DrawString("임시 Iso Grid Render", titleFont, brush, 12, 12);
        for (var i = 0; i < lines.Count; i++)
        {
            graphics.DrawString(lines[i], font, dimBrush, 12, 38 + i * 20);
        }
    }

    private void DrawCenteredText(Graphics graphics, string text)
    {
        using var brush = new SolidBrush(Color.White);
        using var font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        var size = graphics.MeasureString(text, font);
        graphics.DrawString(text, font, brush, (Width - size.Width) / 2, (Height - size.Height) / 2);
    }
}
