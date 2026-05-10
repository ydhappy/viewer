namespace Viewer.App.Map;

public sealed record S32IsoTileLayoutOptions(
    int TileWidth,
    int TileHeight,
    int OriginX,
    int OriginY,
    float Zoom
)
{
    public static S32IsoTileLayoutOptions Default(int originX, int originY, float zoom)
    {
        return new S32IsoTileLayoutOptions(24, 12, originX, originY, zoom);
    }
}

public static class S32IsoTileLayout
{
    public static Point ToScreen(int x, int y, S32IsoTileLayoutOptions options)
    {
        var tileWidth = Math.Max(1, (int)Math.Round(options.TileWidth * options.Zoom));
        var tileHeight = Math.Max(1, (int)Math.Round(options.TileHeight * options.Zoom));

        var screenX = options.OriginX + (x - y) * tileWidth / 2;
        var screenY = options.OriginY + (x + y) * tileHeight / 2;
        return new Point(screenX, screenY);
    }

    public static Rectangle GetImageTarget(int x, int y, S32IsoTileLayoutOptions options)
    {
        var point = ToScreen(x, y, options);
        var width = Math.Max(1, (int)Math.Round(options.TileWidth * options.Zoom));
        var height = Math.Max(1, (int)Math.Round(options.TileWidth * options.Zoom));
        return new Rectangle(point.X - width / 2, point.Y - height / 2, width, height);
    }

    public static Point[] GetDiamond(int x, int y, S32IsoTileLayoutOptions options)
    {
        var point = ToScreen(x, y, options);
        var width = Math.Max(1, (int)Math.Round(options.TileWidth * options.Zoom));
        var height = Math.Max(1, (int)Math.Round(options.TileHeight * options.Zoom));

        return new[]
        {
            new Point(point.X, point.Y),
            new Point(point.X + width / 2, point.Y + height / 2),
            new Point(point.X, point.Y + height),
            new Point(point.X - width / 2, point.Y + height / 2)
        };
    }

    public static Rectangle GetMapBounds(int width, int height, S32IsoTileLayoutOptions options)
    {
        if (width <= 0 || height <= 0)
        {
            return Rectangle.Empty;
        }

        var points = new List<Point>(width * height * 4);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                points.AddRange(GetDiamond(x, y, options));
            }
        }

        var left = points.Min(point => point.X);
        var top = points.Min(point => point.Y);
        var right = points.Max(point => point.X);
        var bottom = points.Max(point => point.Y);
        return Rectangle.FromLTRB(left, top, right, bottom);
    }
}
