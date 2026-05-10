namespace Viewer.App.Pak;

public sealed record L1ImageInfo(
    int Width,
    int Height,
    int XOffset,
    int YOffset,
    string Format,
    string Message
);

public static class L1ImageFormatDecoder
{
    public const int TileBlockSize = 24;

    public static Color Rgb555ToColor(ushort rgb555)
    {
        var b5 = rgb555 & 0x1F;
        var g5 = (rgb555 >> 5) & 0x1F;
        var r5 = (rgb555 >> 10) & 0x1F;
        var r8 = (r5 << 3) | (r5 >> 2);
        var g8 = (g5 << 3) | (g5 >> 2);
        var b8 = (b5 << 3) | (b5 >> 2);
        return Color.FromArgb(r8, g8, b8);
    }

    public static Bitmap RenderTilBlock(byte[] blockData)
    {
        var bitmap = new Bitmap(TileBlockSize, TileBlockSize);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.Transparent);

        if (blockData.Length < 2)
        {
            return bitmap;
        }

        var canvas = DecodeTilBlockToRgb555Canvas(blockData);
        for (var y = 0; y < TileBlockSize; y++)
        {
            for (var x = 0; x < TileBlockSize; x++)
            {
                var rgb555 = canvas[y * TileBlockSize + x];
                if (rgb555 == 0)
                {
                    bitmap.SetPixel(x, y, Color.Transparent);
                }
                else
                {
                    bitmap.SetPixel(x, y, Rgb555ToColor(rgb555));
                }
            }
        }

        return bitmap;
    }

    public static Bitmap RenderTilSheet(IReadOnlyList<byte[]> blocks, int columns = 12)
    {
        if (blocks.Count == 0)
        {
            return new Bitmap(TileBlockSize, TileBlockSize);
        }

        columns = Math.Max(1, columns);
        var rows = (int)Math.Ceiling(blocks.Count / (double)columns);
        var bitmap = new Bitmap(columns * TileBlockSize, rows * TileBlockSize);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.Clear(Color.FromArgb(30, 30, 30));

        for (var i = 0; i < blocks.Count; i++)
        {
            using var block = RenderTilBlock(blocks[i]);
            var x = i % columns * TileBlockSize;
            var y = i / columns * TileBlockSize;
            graphics.DrawImageUnscaled(block, x, y);
        }

        return bitmap;
    }

    public static ushort[] DecodeTilBlockToRgb555Canvas(byte[] blockData)
    {
        var canvas = new ushort[TileBlockSize * TileBlockSize];
        if (blockData.Length < 2)
        {
            return canvas;
        }

        var blockType = blockData[0];
        var isSimpleDiamond = blockType is 0 or 1 or 8 or 9 or 16 or 17;

        if (isSimpleDiamond)
        {
            DecodeSimpleDiamond(blockData, blockType, canvas);
        }
        else
        {
            DecodeSegmentedBlock(blockData, canvas);
        }

        return canvas;
    }

    private static void DecodeSimpleDiamond(byte[] blockData, int blockType, ushort[] canvas)
    {
        int[] rowWidths = { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 22, 24, 22, 20, 18, 16, 14, 12, 10, 8, 6, 4, 2 };
        var dataIndex = 1;

        for (var row = 0; row < rowWidths.Length && dataIndex + 1 < blockData.Length; row++)
        {
            var width = rowWidths[row];
            var startX = (blockType & 1) == 0 ? (TileBlockSize - width) / 2 : 0;

            for (var col = 0; col < width && dataIndex + 1 < blockData.Length; col++)
            {
                var color = (ushort)(blockData[dataIndex] | (blockData[dataIndex + 1] << 8));
                dataIndex += 2;

                var x = startX + col;
                if (x >= 0 && x < TileBlockSize && row >= 0 && row < TileBlockSize)
                {
                    canvas[row * TileBlockSize + x] = color;
                }
            }
        }
    }

    private static void DecodeSegmentedBlock(byte[] blockData, ushort[] canvas)
    {
        if (blockData.Length < 5)
        {
            return;
        }

        var xOffset = blockData[1];
        var yOffset = blockData[2];
        var yLength = blockData[4];
        var index = 5;

        for (var row = 0; row < yLength && index < blockData.Length; row++)
        {
            var segmentCount = blockData[index++];
            var currentX = xOffset;

            for (var segment = 0; segment < segmentCount && index < blockData.Length; segment++)
            {
                if (index + 1 >= blockData.Length)
                {
                    return;
                }

                var skip = blockData[index++];
                var count = blockData[index++];
                currentX += skip / 2;

                for (var pixel = 0; pixel < count && index + 1 < blockData.Length; pixel++)
                {
                    var color = (ushort)(blockData[index] | (blockData[index + 1] << 8));
                    index += 2;

                    var x = currentX + pixel;
                    var y = yOffset + row;
                    if (x >= 0 && x < TileBlockSize && y >= 0 && y < TileBlockSize)
                    {
                        canvas[y * TileBlockSize + x] = color;
                    }
                }

                currentX += count;
            }
        }
    }
}
