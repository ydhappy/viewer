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
        var bytesToRead = GetBytesToRead(record);
        if (bytesToRead <= 0)
        {
            throw new InvalidOperationException(PakRecordDiagnostics.BuildFailureMessage(BuildPackedSizeFailureReason(record), record, pakPath, pakSize));
        }

        if (record.Offset + bytesToRead > input.Length)
        {
            throw new InvalidOperationException(PakRecordDiagnostics.BuildFailureMessage("PAK range overflow: record offset + packed size exceeds PAK file size.", record, pakPath, pakSize));
        }

        input.Seek(record.Offset, SeekOrigin.Begin);
        var data = new byte[bytesToRead];
        try
        {
            ReadExactly(input, data);
        }
        catch (Exception ex)
        {
            throw new IOException(PakRecordDiagnostics.BuildFailureMessage("PAK read failed: could not read the requested record byte range.", record, pakPath, pakSize, ex), ex);
        }

        try
        {
            return DecodeRecordData(data, record);
        }
        catch (Exception ex)
        {
            throw new InvalidDataException(PakRecordDiagnostics.BuildFailureMessage(BuildDecodeFailureReason(record, data), record, pakPath, pakSize, ex), ex);
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

    private static int GetBytesToRead(IdxRecord record)
    {
        return record.Compression == 0 ? record.Size : record.CompressedSize.GetValueOrDefault();
    }

    private static byte[] DecodeRecordData(byte[] data, IdxRecord record)
    {
        var compression = record.Compression == 0 && record.CompressedSize is > 0
            ? DetectExtBCompression(data)
            : record.Compression;

        return compression switch
        {
            0 => TrimUncompressed(data, record.Size),
            1 => DecompressZlib(data, record.Size),
            2 => DecompressBrotli(data, record.Size),
            _ => throw new NotSupportedException($"Unsupported ExtB compression type: {compression}")
        };
    }

    public static int DetectExtBCompression(byte[] header)
    {
        if (header.Length < 2)
        {
            return 0;
        }

        if (header[0] == 0x78 && (header[1] == 0x9C || header[1] == 0xDA || header[1] == 0x01 || header[1] == 0x5E))
        {
            return 1;
        }

        if (header[0] == 0x5B || header[0] == 0x1B)
        {
            return 2;
        }

        return 0;
    }

    private static string BuildPackedSizeFailureReason(IdxRecord record)
    {
        return record.Compression == 0
            ? "Invalid raw record size: unpacked size must be greater than zero."
            : "Packed size missing: compressed record does not have a calculated packed byte size.";
    }

    private static string BuildDecodeFailureReason(IdxRecord record, byte[] data)
    {
        var detectedCompression = DetectExtBCompression(data);
        var effectiveCompression = record.Compression == 0 && record.CompressedSize is > 0
            ? detectedCompression
            : record.Compression;

        return effectiveCompression switch
        {
            0 => "Raw record conversion failed.",
            1 => "Zlib decompression failed: compression type 1 data could not be decoded.",
            2 => "Brotli decompression failed: compression type 2 data could not be decoded.",
            _ => $"Unsupported compression type: {effectiveCompression}."
        };
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
