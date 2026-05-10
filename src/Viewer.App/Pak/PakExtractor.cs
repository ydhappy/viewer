using System.IO.Compression;

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

    public static byte[] ReadBytes(string pakPath, IdxRecord record)
    {
        if (!record.CanExtract)
        {
            throw new InvalidOperationException(PakRecordDiagnostics.BuildFailureMessage("읽기 가능 판정이 된 레코드가 아닙니다.", record, pakPath));
        }

        if (!File.Exists(pakPath))
        {
            throw new FileNotFoundException(PakRecordDiagnostics.BuildFailureMessage("PAK 파일을 찾을 수 없습니다.", record, pakPath), pakPath);
        }

        var pakSize = new FileInfo(pakPath).Length;
        if (record.Size <= 0)
        {
            throw new InvalidOperationException(PakRecordDiagnostics.BuildFailureMessage("읽을 수 있는 레코드 크기가 아닙니다.", record, pakPath, pakSize));
        }

        if (record.Offset < 0)
        {
            throw new InvalidOperationException(PakRecordDiagnostics.BuildFailureMessage("읽을 수 있는 레코드 오프셋이 아닙니다.", record, pakPath, pakSize));
        }

        using var input = new FileStream(pakPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        var bytesToRead = record.Compression == 0 ? record.Size : record.CompressedSize.GetValueOrDefault();
        if (bytesToRead <= 0)
        {
            throw new InvalidOperationException(PakRecordDiagnostics.BuildFailureMessage("압축 레코드의 읽기 크기를 계산할 수 없습니다.", record, pakPath, pakSize));
        }

        if (record.Offset + bytesToRead > input.Length)
        {
            throw new InvalidOperationException(PakRecordDiagnostics.BuildFailureMessage("레코드 범위가 PAK 파일 크기를 초과합니다.", record, pakPath, pakSize));
        }

        input.Seek(record.Offset, SeekOrigin.Begin);
        var data = new byte[bytesToRead];
        try
        {
            ReadExactly(input, data);
        }
        catch (Exception ex)
        {
            throw new IOException(PakRecordDiagnostics.BuildFailureMessage("PAK 레코드 바이트 읽기에 실패했습니다.", record, pakPath, pakSize, ex), ex);
        }

        try
        {
            return record.Compression switch
            {
                0 => TrimUncompressed(data, record.Size),
                1 => DecompressZlib(data, record.Size),
                2 => DecompressBrotli(data, record.Size),
                _ => throw new NotSupportedException($"지원하지 않는 ExtB compression type입니다: {record.Compression}")
            };
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(PakRecordDiagnostics.BuildFailureMessage("PAK 레코드 압축 해제/변환에 실패했습니다.", record, pakPath, pakSize, ex), ex);
        }
    }

    public static string Extract(string pakPath, IdxRecord record, string outputDirectory)
    {
        Directory.CreateDirectory(outputDirectory);

        var safeName = MakeSafeRelativePath(record.FileName);
        var outputPath = Path.Combine(outputDirectory, safeName);
        var outputParent = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(outputParent))
        {
            Directory.CreateDirectory(outputParent);
        }

        var data = ReadBytes(pakPath, record);
        File.WriteAllBytes(outputPath, data);
        return outputPath;
    }

    private static byte[] TrimUncompressed(byte[] data, int expectedSize)
    {
        if (expectedSize > 0 && data.Length > expectedSize)
        {
            var trimmed = new byte[expectedSize];
            Array.Copy(data, trimmed, expectedSize);
            return trimmed;
        }

        return data;
    }

    private static byte[] DecompressZlib(byte[] data, int expectedSize)
    {
        using var input = new MemoryStream(data);
        using var zlib = new ZLibStream(input, CompressionMode.Decompress);
        return CopyDecompressed(zlib, expectedSize);
    }

    private static byte[] DecompressBrotli(byte[] data, int expectedSize)
    {
        using var input = new MemoryStream(data);
        using var brotli = new BrotliStream(input, CompressionMode.Decompress);
        return CopyDecompressed(brotli, expectedSize);
    }

    private static byte[] CopyDecompressed(Stream input, int expectedSize)
    {
        using var output = expectedSize > 0 ? new MemoryStream(expectedSize) : new MemoryStream();
        input.CopyTo(output);
        var result = output.ToArray();
        return TrimUncompressed(result, expectedSize);
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

    private static void ReadExactly(Stream input, byte[] buffer)
    {
        var offset = 0;
        var remaining = buffer.Length;

        while (remaining > 0)
        {
            var read = input.Read(buffer, offset, remaining);
            if (read <= 0)
            {
                throw new EndOfStreamException("PAK 파일을 읽는 중 예상보다 빨리 끝났습니다.");
            }

            offset += read;
            remaining -= read;
        }
    }
}
