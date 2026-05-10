namespace Viewer.App.Map;

public enum S32RenderMode
{
    ColorGrid,
    IsoTile
}

public sealed class S32GridRenderPanel : Panel
{
    private const int DefaultTileImageDrawAttemptsPerPaint = 512;
    private const int MinTileImageDrawAttemptsPerPaint = 0;
    private const int MaxTileImageDrawAttemptsPerPaint = 4096;

    private readonly DefaultTileImageCache _tileImageCache = new();

    private S32Info? _currentMap;
    private TileResourceSet? _tileResourceSet;
    private S32LayerSample? _layerSample;
    private Rectangle _lastGridBounds = Rectangle.Empty;
    private int _lastCellSize;
    private Point? _hoverTile;
    private Point? _selectedTile;
    private float _zoom = 1.0f;
    private int _lastTileImageSuccessCount;
    private int _lastTileImageAttemptCount;
    private int _lastVisibleTileCount;
    private int _lastDrawnTileCount;
    private int _lastSkippedTileCount;
    private int _tileImageDrawAttemptLimit = DefaultTileImageDrawAttemptsPerPaint;
    private bool _tileImageRenderEnabled = true;
    private S32RenderMode _renderMode = S32RenderMode.ColorGrid;
    private Point _isoPanOffset = Point.Empty;
    private bool _isPanning;
    private Point _lastPanMouseLocation;

    public S32GridRenderPanel()
    {
        DoubleBuffered = true;
        BackColor = Color.FromArgb(24, 24, 24);
        SetStyle(ControlStyles.Selectable, true);
        TabStop = true;
        ContextMenuStrip = BuildContextMenu();
    }

    public float Zoom => _zoom;

    public S32RenderMode RenderMode
    {
        get => _renderMode;
        set
        {
            if (_renderMode == value)
            {
                return;
            }

            _renderMode = value;
            _hoverTile = null;
            _selectedTile = null;
            Invalidate();
        }
    }

    public bool TileImageRenderEnabled
    {
        get => _tileImageRenderEnabled;
        set
        {
            if (_tileImageRenderEnabled == value)
            {
                return;
            }

            _tileImageRenderEnabled = value;
            Invalidate();
        }
    }

    public int TileImageDrawAttemptLimit
    {
        get => _tileImageDrawAttemptLimit;
        set
        {
            _tileImageDrawAttemptLimit = Math.Clamp(value, MinTileImageDrawAttemptsPerPaint, MaxTileImageDrawAttemptsPerPaint);
            Invalidate();
        }
    }

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
            $"Mode     : {_renderMode}",
            $"Zoom     : {_zoom:0.00}x",
            $"Pan      : x={_isoPanOffset.X}, y={_isoPanOffset.Y}",
            $"Hover    : {FormatTile(_hoverTile, hoverTileId)}",
            $"Selected : {FormatTile(_selectedTile, selectedTileId)}",
            $"Tile.idx : {(_tileResourceSet is null ? "not loaded" : _tileResourceSet.IdxPath)}",
            $"TileImage: enabled={_tileImageRenderEnabled}, limit={_tileImageDrawAttemptLimit:N0}, attempts={_lastTileImageAttemptCount:N0}, success={_lastTileImageSuccessCount:N0}",
            $"Viewport : visible={_lastVisibleTileCount:N0}, drawn={_lastDrawnTileCount:N0}, skipped={_lastSkippedTileCount:N0}");
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

    public void ResetPan()
    {
        _isoPanOffset = Point.Empty;
        Invalidate();
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
        ResetRenderCounters();
        _isoPanOffset = Point.Empty;

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
        ResetRenderCounters();
        Invalidate();
    }

    protected override void OnMouseMove(MouseEventArgs e)
    {
        base.OnMouseMove(e);
        if (_renderMode == S32RenderMode.IsoTile && _isPanning)
        {
            _isoPanOffset = new Point(
                _isoPanOffset.X + e.Location.X - _lastPanMouseLocation.X,
                _isoPanOffset.Y + e.Location.Y - _lastPanMouseLocation.Y);
            _lastPanMouseLocation = e.Location;
            Invalidate();
            return;
        }

        _hoverTile = TryGetTileAt(e.Location);
        Invalidate();
    }

    protected override void OnMouseLeave(EventArgs e)
    {
        base.OnMouseLeave(e);
        _hoverTile = null;
        _isPanning = false;
        Invalidate();
    }

    protected override void OnMouseDown(MouseEventArgs e)
    {
        base.OnMouseDown(e);
        if (_renderMode == S32RenderMode.IsoTile && e.Button == MouseButtons.Middle)
        {
            _isPanning = true;
            _lastPanMouseLocation = e.Location;
            Cursor = Cursors.SizeAll;
        }
    }

    protected override void OnMouseUp(MouseEventArgs e)
    {
        base.OnMouseUp(e);
        if (_isPanning && e.Button == MouseButtons.Middle)
        {
            _isPanning = false;
            Cursor = Cursors.Default;
        }
    }

    protected override void OnMouseClick(MouseEventArgs e)
    {
        base.OnMouseClick(e);
        Focus();
        if (e.Button == MouseButtons.Left)
        {
            _selectedTile = TryGetTileAt(e.Location);
            Invalidate();
        }
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
        e.Graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
        e.Graphics.PixelOffsetMode = System.Drawing.Drawing2D.PixelOffsetMode.Half;

        if (_currentMap is null)
        {
            DrawCenteredText(e.Graphics, "S32 파일을 선택하세요.");
            return;
        }

        var clipBounds = Rectangle.Ceiling(e.Graphics.VisibleClipBounds);
        if (_layerSample?.HasLayer1 == true)
        {
            if (_renderMode == S32RenderMode.IsoTile)
            {
                DrawLayer1IsoTiles(e.Graphics, _layerSample, clipBounds);
            }
            else
            {
                DrawLayer1ColorGrid(e.Graphics, _layerSample, clipBounds);
            }
        }
        else
        {
            ResetRenderCounters();
            DrawIsoGrid(e.Graphics);
        }

        DrawOverlay(e.Graphics);
    }

    private ContextMenuStrip BuildContextMenu()
    {
        var menu = new ContextMenuStrip();
        var colorGridMode = new ToolStripMenuItem("Render Mode: Color Grid") { CheckOnClick = true, Checked = true };
        var isoTileMode = new ToolStripMenuItem("Render Mode: Iso Tile") { CheckOnClick = true };
        colorGridMode.Click += (_, _) =>
        {
            RenderMode = S32RenderMode.ColorGrid;
            colorGridMode.Checked = true;
            isoTileMode.Checked = false;
        };
        isoTileMode.Click += (_, _) =>
        {
            RenderMode = S32RenderMode.IsoTile;
            colorGridMode.Checked = false;
            isoTileMode.Checked = true;
        };

        var toggleTileImages = new ToolStripMenuItem("Tile Image Render 켜기/끄기")
        {
            CheckOnClick = true,
            Checked = true
        };
        toggleTileImages.Click += (_, _) => TileImageRenderEnabled = toggleTileImages.Checked;

        var lowerLimit = new ToolStripMenuItem("Tile Image Limit 낮추기") { ShortcutKeyDisplayString = "-256" };
        lowerLimit.Click += (_, _) => TileImageDrawAttemptLimit -= 256;

        var raiseLimit = new ToolStripMenuItem("Tile Image Limit 올리기") { ShortcutKeyDisplayString = "+256" };
        raiseLimit.Click += (_, _) => TileImageDrawAttemptLimit += 256;

        var resetLimit = new ToolStripMenuItem("Tile Image Limit 기본값") { ShortcutKeyDisplayString = DefaultTileImageDrawAttemptsPerPaint.ToString("N0") };
        resetLimit.Click += (_, _) => TileImageDrawAttemptLimit = DefaultTileImageDrawAttemptsPerPaint;

        var resetZoom = new ToolStripMenuItem("Zoom 100%") { ShortcutKeyDisplayString = "Reset" };
        resetZoom.Click += (_, _) => ResetZoom();

        var resetPan = new ToolStripMenuItem("Pan Reset") { ShortcutKeyDisplayString = "Middle drag" };
        resetPan.Click += (_, _) => ResetPan();

        menu.Items.Add(colorGridMode);
        menu.Items.Add(isoTileMode);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(toggleTileImages);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(lowerLimit);
        menu.Items.Add(raiseLimit);
        menu.Items.Add(resetLimit);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(resetZoom);
        menu.Items.Add(resetPan);
        return menu;
    }

    private void DrawLayer1ColorGrid(Graphics graphics, S32LayerSample sample, Rectangle clipBounds)
    {
        var baseCellSize = Math.Max(3, Math.Min(8, Math.Min(Width / S32LayerSample.Width, Math.Max(1, (Height - 140) / S32LayerSample.Height))));
        var cellSize = Math.Max(2, (int)Math.Round(baseCellSize * _zoom));
        var gridWidth = S32LayerSample.Width * cellSize;
        var gridHeight = S32LayerSample.Height * cellSize;
        var startX = Math.Max(12, (Width - gridWidth) / 2);
        var startY = 140;

        _lastCellSize = cellSize;
        _lastGridBounds = new Rectangle(startX, startY, gridWidth, gridHeight);
        ResetRenderCounters();

        var canAttemptTileImages = _tileImageRenderEnabled && _tileResourceSet is not null && cellSize >= 8 && _tileImageDrawAttemptLimit > 0;

        for (var y = 0; y < S32LayerSample.Height; y++)
        {
            for (var x = 0; x < S32LayerSample.Width; x++)
            {
                var target = new Rectangle(startX + x * cellSize, startY + y * cellSize, cellSize, cellSize);
                if (!clipBounds.IntersectsWith(target))
                {
                    _lastSkippedTileCount++;
                    continue;
                }

                _lastVisibleTileCount++;
                var tileId = sample.GetTileId(x, y);
                var imageDrawn = false;

                if (canAttemptTileImages && _lastTileImageAttemptCount < _tileImageDrawAttemptLimit)
                {
                    imageDrawn = TryDrawTileImage(graphics, tileId, target);
                }

                if (!imageDrawn)
                {
                    using var brush = new SolidBrush(BuildTileColor(tileId));
                    graphics.FillRectangle(brush, target);
                }

                _lastDrawnTileCount++;
            }
        }

        using var borderPen = new Pen(Color.FromArgb(180, 220, 220, 220));
        graphics.DrawRectangle(borderPen, _lastGridBounds);

        DrawColorGridTileMarker(graphics, _hoverTile, Color.White, 1);
        DrawColorGridTileMarker(graphics, _selectedTile, Color.Yellow, 2);
    }

    private void DrawLayer1IsoTiles(Graphics graphics, S32LayerSample sample, Rectangle clipBounds)
    {
        _lastGridBounds = Rectangle.Empty;
        _lastCellSize = 0;
        ResetRenderCounters();

        var options = CreateIsoLayoutOptions();
        var canAttemptTileImages = _tileImageRenderEnabled && _tileResourceSet is not null && _tileImageDrawAttemptLimit > 0;

        using var gridPen = new Pen(Color.FromArgb(80, 180, 180, 180));
        for (var y = 0; y < S32LayerSample.Height; y++)
        {
            for (var x = 0; x < S32LayerSample.Width; x++)
            {
                var diamond = S32IsoTileLayout.GetDiamond(x, y, options);
                var target = S32IsoTileLayout.GetImageTarget(x, y, options);
                var tileBounds = Rectangle.Union(GetPolygonBounds(diamond), target);
                if (!clipBounds.IntersectsWith(tileBounds))
                {
                    _lastSkippedTileCount++;
                    continue;
                }

                _lastVisibleTileCount++;
                var tileId = sample.GetTileId(x, y);
                var imageDrawn = false;

                if (canAttemptTileImages && _lastTileImageAttemptCount < _tileImageDrawAttemptLimit)
                {
                    imageDrawn = TryDrawTileImage(graphics, tileId, target);
                }

                if (!imageDrawn)
                {
                    using var brush = new SolidBrush(BuildTileColor(tileId));
                    graphics.FillPolygon(brush, diamond);
                    graphics.DrawPolygon(gridPen, diamond);
                }

                _lastDrawnTileCount++;
            }
        }

        DrawIsoTileMarker(graphics, _hoverTile, Color.White, 1, options);
        DrawIsoTileMarker(graphics, _selectedTile, Color.Yellow, 2, options);
    }

    private S32IsoTileLayoutOptions CreateIsoLayoutOptions()
    {
        return S32IsoTileLayoutOptions.Default(Width / 2 + _isoPanOffset.X, 120 + _isoPanOffset.Y, _zoom);
    }

    private bool TryDrawTileImage(Graphics graphics, ushort tileId, Rectangle target)
    {
        if (_tileResourceSet is null || tileId == 0)
        {
            return false;
        }

        _lastTileImageAttemptCount++;
        TileConversionResult result;
        try
        {
            result = _tileImageCache.GetTileImage(tileId, _tileResourceSet);
        }
        catch
        {
            return false;
        }

        if (!result.Success || result.Image is null)
        {
            return false;
        }

        graphics.DrawImage(result.Image, target);
        _lastTileImageSuccessCount++;
        return true;
    }

    private void DrawColorGridTileMarker(Graphics graphics, Point? tilePoint, Color color, int width)
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

    private static void DrawIsoTileMarker(Graphics graphics, Point? tilePoint, Color color, int width, S32IsoTileLayoutOptions options)
    {
        if (tilePoint is null)
        {
            return;
        }

        var point = tilePoint.Value;
        if (point.X < 0 || point.X >= S32LayerSample.Width || point.Y < 0 || point.Y >= S32LayerSample.Height)
        {
            return;
        }

        using var pen = new Pen(color, width);
        graphics.DrawPolygon(pen, S32IsoTileLayout.GetDiamond(point.X, point.Y, options));
    }

    private Point? TryGetTileAt(Point location)
    {
        if (_layerSample?.HasLayer1 != true)
        {
            return null;
        }

        if (_renderMode == S32RenderMode.IsoTile)
        {
            return S32IsoTileLayout.TryFromScreenCandidate(location, S32LayerSample.Width, S32LayerSample.Height, CreateIsoLayoutOptions());
        }

        if (_lastCellSize <= 0 || !_lastGridBounds.Contains(location))
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
        var tileImageText = _tileResourceSet is null
            ? "Tile image render: unavailable"
            : $"Tile image render: enabled={_tileImageRenderEnabled}, attempts={_lastTileImageAttemptCount:N0}, success={_lastTileImageSuccessCount:N0}, limit={_tileImageDrawAttemptLimit:N0}";

        var lines = new List<string>
        {
            $"S32: {_currentMap!.FileName}",
            $"Coord: {(_currentMap.Coordinate is null ? "unknown" : _currentMap.Coordinate.ToString())}",
            $"Size: {_currentMap.FileSize:N0} bytes",
            $"Mode: {_renderMode}",
            $"Pan: x={_isoPanOffset.X}, y={_isoPanOffset.Y}",
            layerText,
            $"Viewport: visible={_lastVisibleTileCount:N0}, drawn={_lastDrawnTileCount:N0}, skipped={_lastSkippedTileCount:N0}",
            $"Zoom: {_zoom:0.00}x",
            $"Hover: {FormatTile(_hoverTile, hoverTileId)}",
            $"Selected: {FormatTile(_selectedTile, selectedTileId)}",
            $"Tile.idx: {(_tileResourceSet is null ? "not loaded" : _tileResourceSet.ExtractableRecords + "/" + _tileResourceSet.TotalRecords)}",
            tileImageText,
            "Right click: render options / Middle drag: pan"
        };

        graphics.DrawString(_layerSample?.HasLayer1 == true ? "Layer1 Tile Render" : "임시 Iso Grid Render", titleFont, brush, 12, 12);
        for (var i = 0; i < lines.Count; i++)
        {
            graphics.DrawString(lines[i], font, dimBrush, 12, 38 + i * 20);
        }
    }

    private void ResetRenderCounters()
    {
        _lastTileImageAttemptCount = 0;
        _lastTileImageSuccessCount = 0;
        _lastVisibleTileCount = 0;
        _lastDrawnTileCount = 0;
        _lastSkippedTileCount = 0;
    }

    private static Rectangle GetPolygonBounds(IReadOnlyList<Point> points)
    {
        if (points.Count == 0)
        {
            return Rectangle.Empty;
        }

        var left = points.Min(point => point.X);
        var top = points.Min(point => point.Y);
        var right = points.Max(point => point.X);
        var bottom = points.Max(point => point.Y);
        return Rectangle.FromLTRB(left, top, right, bottom);
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
