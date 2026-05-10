namespace Viewer.App.Map;

public sealed class S32GridRenderPanel : Panel
{
    private S32Info? _currentMap;
    private TileResourceSet? _tileResourceSet;
    private S32LayerSample? _layerSample;
    private Rectangle _lastGridBounds = Rectangle.Empty;
    private int _lastCellSize;
    private Point? _hoverTile;
    private Point? _selectedTile;
    private float _zoom = 1.0f;

    public S32GridRenderPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(24, 24, 24);
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
    }

    public float Zoom => _zoom;

    public Bitmap CreateSnapshot()
    {
        var width = Math.Max(1, Width);
        var height = Math.Max(1, Height);
        var bitmap = new Bitmap(width, height);
        DrawToBitmap(bitmap, new Rectangle(0, 0, width, height));
        return bitmap;
    }

    public string GetSelectedTileInfoText()
    {
        var selectedTileId = GetTileId(_selectedTile);
        var hoverTileId = GetTileId(_hoverTile);

        return string.Join(Environment.NewLine,
            "S32 Render Tile Info",
            "====================",
            $"Map      : {(_currentMap is null ? "-" : _currentMap.FileName)}",
            $"Path     : {(_currentMap is null ? "-" : _currentMap.FilePath)}",
            $"Zoom     : {_zoom:0.00}x",
            $"Hover    : {FormatTile(_hoverTile, hoverTileId)}",
            $"Selected : {FormatTile(_selectedTile, selectedTileId)}",
            $"Tile.idx : {(_tileResourceSet is null ? "not loaded" : _tileResourceSet.IdxPath)}");
    }

    public void ZoomIn()
    {
        SetZoom(_zoom + 0.25f);
    }

    public void ZoomOut()
    {
        SetZoom(_zoom - 0.25f);
    }

    public void ResetZoom()
    {
        SetZoom(1.0f);
    }

    public void SetZoom(float zoom)
    {
        _zoom = Math.Clamp(zoom, 0.5f, 4.0f);
        Invalidate();
    }

    public void SetMap(S32Info? mapInfo)
    {
        _currentMap = mapInfo;
        _layerSample = null;
        _hoverTile = null;
        _selectedTile = null;

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

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        _hoverTile = TryGetTileAt(e.Location);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoverTile = null;
        Invalidate();
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        Focus();
        _selectedTile = TryGetTileAt(e.Location);
        Invalidate();
    }

    protected override void OnMouseWheel(MouseEventArgs e)
    {
        base.OnMouseWheel(e);
        if (e.Delta > 0)
        {
            ZoomIn();
        }
        else if (e.Delta < 0)
        {
            ZoomOut();
        }
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
        var baseCellSize = Math.Max(3, Math.Min(8, Math.Min(Width / S32LayerSample.Width, Math.Max(1, (Height - 140) / S32LayerSample.Height))));
        var cellSize = Math.Max(2, (int)Math.Round(baseCellSize * _zoom));
        var gridWidth = S32LayerSample.Width * cellSize;
        var gridHeight = S32LayerSample.Height * cellSize;
        var startX = Math.Max(12, (Width - gridWidth) / 2);
        var startY = 140;

        _lastCellSize = cellSize;
        _lastGridBounds = new Rectangle(startX, startY, gridWidth, gridHeight);

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
        graphics.DrawRectangle(borderPen, _lastGridBounds);

        DrawTileMarker(graphics, _hoverTile, Color.White, 1);
        DrawTileMarker(graphics, _selectedTile, Color.Yellow, 2);
    }

    private void DrawTileMarker(Graphics graphics, Point? tilePoint, Color color, int width)
    {
        if (tilePoint is null || _lastCellSize <= 0 || _lastGridBounds.IsEmpty)
        {
            return;
        }

        var point = tilePoint.Value;
        if (point.X < 0 || point.X >= S32LayerSample.Width || point.Y < 0 || point.Y >= S32LayerSample.Height)
        {
            return;
        }

        using var pen = new Pen(color, width);
        graphics.DrawRectangle(
            pen,
            _lastGridBounds.Left + point.X * _lastCellSize,
            _lastGridBounds.Top + point.Y * _lastCellSize,
            _lastCellSize,
            _lastCellSize);
    }

    private Point? TryGetTileAt(Point location)
    {
        if (_layerSample?.HasLayer1 != true || _lastCellSize <= 0 || !_lastGridBounds.Contains(location))
        {
            return null;
        }

        var x = (location.X - _lastGridBounds.Left) / _lastCellSize;
        var y = (location.Y - _lastGridBounds.Top) / _lastCellSize;
        if (x < 0 || x >= S32LayerSample.Width || y < 0 || y >= S32LayerSample.Height)
        {
            return null;
        }

        return new Point(x, y);
    }

    private ushort? GetTileId(Point? tilePoint)
    {
        if (tilePoint is null || _layerSample?.HasLayer1 != true)
        {
            return null;
        }

        return _layerSample.GetTileId(tilePoint.Value.X, tilePoint.Value.Y);
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
        _lastGridBounds = Rectangle.Empty;
        _lastCellSize = 0;

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

        var hoverTileId = GetTileId(_hoverTile);
        var selectedTileId = GetTileId(_selectedTile);

        var lines = new List<string>
        {
            $"S32: {_currentMap!.FileName}",
            $"Coord: {(_currentMap.Coordinate is null ? "unknown" : _currentMap.Coordinate.ToString())}",
            $"Size: {_currentMap.FileSize:N0} bytes",
            layerText,
            $"Zoom: {_zoom:0.00}x",
            $"Hover: {FormatTile(_hoverTile, hoverTileId)}",
            $"Selected: {FormatTile(_selectedTile, selectedTileId)}",
            $"Tile.idx: {(_tileResourceSet is null ? "not loaded" : _tileResourceSet.ExtractableRecords + "/" + _tileResourceSet.TotalRecords)}"
        };

        graphics.DrawString(_layerSample?.HasLayer1 == true ? "Layer1 Tile ID Color Grid" : "임시 Iso Grid Render", titleFont, brush, 12, 12);
        for (var i = 0; i < lines.Count; i++)
        {
            graphics.DrawString(lines[i], font, dimBrush, 12, 38 + i * 20);
        }
    }

    private static string FormatTile(Point? point, ushort? tileId)
    {
        if (point is null || tileId is null)
        {
            return "-";
        }

        return $"x={point.Value.X}, y={point.Value.Y}, tileId={tileId.Value}";
    }

    private void DrawCenteredText(Graphics graphics, string text)
    {
        using var brush = new SolidBrush(Color.White);
        using var font = new Font(FontFamily.GenericSansSerif, 12, FontStyle.Bold);
        var size = graphics.MeasureString(text, font);
        graphics.DrawString(text, font, brush, (Width - size.Width) / 2, (Height - size.Height) / 2);
    }
}
