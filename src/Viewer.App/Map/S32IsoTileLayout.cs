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
        var tileWidth = GetScaledTileWidth(options);
        var tileHeight = GetScaledTileHeight(options);

        var screenX = options.OriginX + (x - y) * tileWidth / 2;
        var screenY = options.OriginY + (x + y) * tileHeight / 2;
        return new Point(screenX, screenY);
    }

    public static Point? TryFromScreenCandidate(Point screenPoint, int mapWidth, int mapHeight, S32IsoTileLayoutOptions options)
    {
        var tileWidth = GetScaledTileWidth(options);
        var tileHeight = GetScaledTileHeight(options);
        var halfWidth = tileWidth / 2.0;
        var halfHeight = tileHeight / 2.0;

        if (halfWidth <= 0 || halfHeight <= 0)
        {
            return null;
        }

        var dx = screenPoint.X - options.OriginX;
        var dy = screenPoint.Y - options.OriginY;
        var a = dx / halfWidth;
        var b = dy / halfHeight;
        var xFloat = (a + b) / 2.0;
        var yFloat = (b - a) / 2.0;

        var candidates = new[]
        {
            new Point((int)Math.Floor(xFloat), (int)Math.Floor(yFloat)),
            new Point((int)Math.Round(xFloat), (int)Math.Round(yFloat)),
            new Point((int)Math.Ceiling(xFloat), (int)Math.Ceiling(yFloat))
        };

        foreach (var candidate in candidates.Distinct())
        {
            if (candidate.X >= 0 && candidate.X < mapWidth && candidate.Y >= 0 && candidate.Y < mapHeight &&
                IsPointInsideDiamond(screenPoint, candidate.X, candidate.Y, options))
            {
                return candidate;
            }
        }

        return null;
    }

    public static Rectangle GetImageTarget(int x, int y, S32IsoTileLayoutOptions options)
    {
        var point = ToScreen(x, y, options);
        var width = GetScaledTileWidth(options);
        var height = GetScaledTileWidth(options);
        return new Rectangle(point.X - width / 2, point.Y - height / 2, width, height);
    }

    public static Point[] GetDiamond(int x, int y, S32IsoTileLayoutOptions options)
    {
        var point = ToScreen(x, y, options);
        var width = GetScaledTileWidth(options);
        var height = GetScaledTileHeight(options);

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

    private static bool IsPointInsideDiamond(Point point, int x, int y, S32IsoTileLayoutOptions options)
    {
        var center = ToScreen(x, y, options);
        var width = GetScaledTileWidth(options);
        var height = GetScaledTileHeight(options);
        var halfWidth = Math.Max(1.0, width / 2.0);
        var halfHeight = Math.Max(1.0, height / 2.0);
        var diamondCenterY = center.Y + halfHeight;
        var normalized = Math.Abs(point.X - center.X) / halfWidth + Math.Abs(point.Y - diamondCenterY) / halfHeight;
        return normalized <= 1.0;
    }

    private static int GetScaledTileWidth(S32IsoTileLayoutOptions options)
    {
        return Math.Max(1, (int)Math.Round(options.TileWidth * options.Zoom));
    }

    private static int GetScaledTileHeight(S32IsoTileLayoutOptions options)
    {
        return Math.Max(1, (int)Math.Round(options.TileHeight * options.Zoom));
    }
}
