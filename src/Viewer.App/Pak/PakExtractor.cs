namespace Viewer.App.Pak;

public static class PakExtractor
{
    public static string ResolvePakPath(string idxPath)
    {
        var directory = Path.GetDirectoryName(idxPath) ?? string.Empty;
        var fileNameWithoutExt = Path.GetFileNameWithoutExtension(idxPath);
        var pakPath = Path.Combine(directory, fileNameWithoutExt + ".pak");

        if (File.Exists(pakPath))
        {
            return pakPath;
        }

        var upperPakPath = Path.Combine(directory, fileNameWithoutExt + ".PAK");
        if (File.Exists(upperPakPath))
        {
            return upperPakPath;
        }

        return pakPath;
    }

    public static string Extract(string pakPath, IdxRecord record, string outputDirectory)
    {
        if (!File.Exists(pakPath))
        {
            throw new FileNotFoundException("PAK 파일을 찾을 수 없습니다.", pakPath);
        }

        if (record.Size <= 0)
        {
            throw new InvalidOperationException("추출 가능한 레코드 크기가 아닙니다.");
        }

        if (record.Offset < 0)
        {
            throw new InvalidOperationException("추출 가능한 레코드 오프셋이 아닙니다.");
        }

        Directory.CreateDirectory(outputDirectory);

        var safeName = MakeSafeRelativePath(record.FileName);
        var outputPath = Path.Combine(outputDirectory, safeName);
        var outputParent = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputParent))
        {
            Directory.CreateDirectory(outputParent);
        }

        using var input = new FileStream(pakPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        if (record.Offset + record.Size > input.Length)
        {
            throw new InvalidOperationException("레코드 범위가 PAK 파일 크기를 초과합니다.");
        }

        input.Seek(record.Offset, SeekOrigin.Begin);
        using var output = new FileStream(outputPath, FileMode.Create, FileAccess.Write, FileShare.None);
        CopyExactly(input, output, record.Size);
        return outputPath;
    }

    private static string MakeSafeRelativePath(string fileName)
    {
        var normalized = fileName.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
        var invalidChars = Path.GetInvalidFileNameChars();
        var parts = normalized
            .Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries)
            .Where(part => part != "." && part != "..")
            .Select(part => new string(part.Select(ch => invalidChars.Contains(ch) ? '_' : ch).ToArray()))
            .ToArray();

        return parts.Length == 0 ? "unknown.bin" : Path.Combine(parts);
    }

    private static void CopyExactly(Stream input, Stream output, int bytesToCopy)
    {
        var buffer = new byte[64 * 1024];
        var remaining = bytesToCopy;

        while (remaining > 0)
        {
            var read = input.Read(buffer, 0, Math.Min(buffer.Length, remaining));
            if (read <= 0)
            {
                throw new EndOfStreamException("PAK 파일을 읽는 중 예상보다 빨리 끝났습니다.");
            }

            output.Write(buffer, 0, read);
            remaining -= read;
        }
    }
}
