namespace Viewer.App.Map;

public sealed class S32GridRenderPanel : Panel
{
    private S32Info? _currentMap;
    private TileResourceSet? _tileResourceSet;
    private S32LayerSample? _layerSample;

    public S32GridRenderPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(24, 24, 24);
    }

    public void SetMap(S32Info? mapInfo)
    {
        _currentMap = mapInfo;
        _layerSample = null;

        if (mapInfo is not null)
        {
            try
            {
                _layerSample = S32LayerParser.ParseLayer1Sample(mapInfo.FilePath);
            }
            catch
            {
                _layerSample = null;
            }
        }

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

        if (_layerSample?.HasLayer1 == true)
        {
            DrawLayer1ColorGrid(e.Graphics, _layerSample);
        }
        else
        {
            DrawIsoGrid(e.Graphics);
        }

        DrawOverlay(e.Graphics);
    }

    private void DrawLayer1ColorGrid(Graphics graphics, S32LayerSample sample)
    {
        var cellSize = Math.Max(3, Math.Min(8, Math.Min(Width / S32LayerSample.Width, Math.Max(1, (Height - 120) / S32LayerSample.Height))));
        var gridWidth = S32LayerSample.Width * cellSize;
        var gridHeight = S32LayerSample.Height * cellSize;
        var startX = Math.Max(12, (Width - gridWidth) / 2);
        var startY = 120;

        for (var y = 0; y < S32LayerSample.Height; y++)
        {
            for (var x = 0; x < S32LayerSample.Width; x++)
            {
                var tileId = sample.GetTileId(x, y);
                using var brush = new SolidBrush(BuildTileColor(tileId));
                graphics.FillRectangle(brush, startX + x * cellSize, startY + y * cellSize, cellSize, cellSize);
            }
        }

        using var borderPen = new Pen(Color.FromArgb(180, 220, 220, 220));
        graphics.DrawRectangle(borderPen, startX, startY, gridWidth, gridHeight);
    }

    private static Color BuildTileColor(ushort tileId)
    {
        if (tileId == 0)
        {
            return Color.FromArgb(45, 45, 45);
        }

        var r = 40 + tileId * 37 % 180;
        var g = 40 + tileId * 57 % 180;
        var b = 40 + tileId * 83 % 180;
        return Color.FromArgb(r, g, b);
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

        var layerText = _layerSample?.HasLayer1 == true
            ? $"Layer1 sample: {_layerSample.Count:N0} tile IDs / {_layerSample.BytesRead:N0} bytes"
            : "Layer1 sample: unavailable";

        var lines = new List<string>
        {
            $"S32: {_currentMap!.FileName}",
            $"Coord: {(_currentMap.Coordinate is null ? "unknown" : _currentMap.Coordinate.ToString())}",
            $"Size: {_currentMap.FileSize:N0} bytes",
            layerText,
            $"Tile.idx: {(_tileResourceSet is null ? "not loaded" : _tileResourceSet.ExtractableRecords + "/" + _tileResourceSet.TotalRecords)}"
        };

        graphics.DrawString(_layerSample?.HasLayer1 == true ? "Layer1 Tile ID Color Grid" : "임시 Iso Grid Render", titleFont, brush, 12, 12);
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
