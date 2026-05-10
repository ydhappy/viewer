namespace Viewer.App.Pak;

public sealed record L1TilBlock(
    int Index,
    int Offset,
    int Length,
    int BlockType,
    byte[] Data
);

public static class L1TilBlockParser
{
    private const int MaxBlocks = 4096;
    private const int SimpleDiamondLength = 1 + 288 * 2;

    public static IReadOnlyList<L1TilBlock> ParseBlocks(byte[] data)
    {
        var blocks = new List<L1TilBlock>();
        var offset = 0;

        while (offset < data.Length && blocks.Count < MaxBlocks)
        {
            if (!TryGetBlockLength(data, offset, out var length))
            {
                break;
            }

            if (length <= 0 || offset + length > data.Length)
            {
                break;
            }

            var blockData = new byte[length];
            Array.Copy(data, offset, blockData, 0, length);
            blocks.Add(new L1TilBlock(
                Index: blocks.Count,
                Offset: offset,
                Length: length,
                BlockType: blockData[0],
                Data: blockData));
            offset += length;
        }

        if (blocks.Count == 0 && data.Length > 0)
        {
            blocks.Add(new L1TilBlock(0, 0, data.Length, data[0], data));
        }

        return blocks;
    }

    public static string BuildSummary(IReadOnlyList<L1TilBlock> blocks, int totalBytes)
    {
        if (blocks.Count == 0)
        {
            return "No TIL blocks parsed.";
        }

        var parsedBytes = blocks.Sum(block => block.Length);
        var byType = blocks
            .GroupBy(block => block.BlockType)
            .OrderBy(group => group.Key)
            .Select(group => $"- type {group.Key}: {group.Count():N0}");

        return string.Join(Environment.NewLine,
            "L1 TIL Block Parse",
            "==================",
            $"Total bytes : {totalBytes:N0}",
            $"Parsed bytes: {parsedBytes:N0}",
            $"Blocks      : {blocks.Count:N0}",
            string.Empty,
            "Block Types",
            "-----------",
            string.Join(Environment.NewLine, byType));
    }

    private static bool TryGetBlockLength(byte[] data, int offset, out int length)
    {
        length = 0;
        if (offset < 0 || offset >= data.Length)
        {
            return false;
        }

        var blockType = data[offset];
        if (IsSimpleDiamondBlockType(blockType))
        {
            if (offset + SimpleDiamondLength <= data.Length)
            {
                length = SimpleDiamondLength;
                return true;
            }

            length = data.Length - offset;
            return length > 0;
        }

        return TryGetSegmentedBlockLength(data, offset, out length);
    }

    private static bool TryGetSegmentedBlockLength(byte[] data, int offset, out int length)
    {
        length = 0;
        if (offset + 5 > data.Length)
        {
            return false;
        }

        var yLength = data[offset + 4];
        if (yLength <= 0 || yLength > L1ImageFormatDecoder.TileBlockSize)
        {
            return false;
        }

        var index = offset + 5;
        for (var row = 0; row < yLength; row++)
        {
            if (index >= data.Length)
            {
                return false;
            }

            var segmentCount = data[index++];
            if (segmentCount > 32)
            {
                return false;
            }

            for (var segment = 0; segment < segmentCount; segment++)
            {
                if (index + 2 > data.Length)
                {
                    return false;
                }

                _ = data[index++];
                var count = data[index++];
                var pixelBytes = count * 2;
                if (index + pixelBytes > data.Length)
                {
                    return false;
                }

                index += pixelBytes;
            }
        }

        length = index - offset;
        return length > 0;
    }

    private static bool IsSimpleDiamondBlockType(int blockType)
    {
        return blockType is 0 or 1 or 8 or 9 or 16 or 17;
    }
}
