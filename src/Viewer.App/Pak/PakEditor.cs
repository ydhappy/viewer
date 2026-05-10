namespace Viewer.App.Pak;

public sealed record PakEditResult(
    bool Success,
    string Message,
    string PakPath,
    string FileName,
    int Offset,
    int Size,
    string? BackupPath = null
);

public static class PakEditor
{
    public static PakEditResult ImportSameSize(string pakPath, IdxRecord record, string inputFilePath, bool createBackup = true)
    {
        if (!File.Exists(inputFilePath))
        {
            var missingInput = Fail("Input file does not exist.", pakPath, record);
            PakEditDiagnostics.AppendResult("same-size-import", missingInput);
            return missingInput;
        }

        var inputBytes = File.ReadAllBytes(inputFilePath);
        return UpdateSameSize(pakPath, record, inputBytes, createBackup);
    }

    public static PakEditResult UpdateSameSize(string pakPath, IdxRecord record, byte[] replacementBytes, bool createBackup = true)
    {
        var validation = ValidateSameSizeUpdate(pakPath, record, replacementBytes.Length);
        if (!validation.Success)
        {
            PakEditDiagnostics.AppendResult("same-size-update", validation);
            return validation;
        }

        string? backupPath = null;
        PakEditResult result;
        try
        {
            if (createBackup)
            {
                backupPath = CreateBackup(pakPath);
            }

            using var pak = new FileStream(pakPath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
            pak.Seek(record.Offset, SeekOrigin.Begin);
            pak.Write(replacementBytes, 0, replacementBytes.Length);
            pak.Flush(flushToDisk: true);

            result = new PakEditResult(
                true,
                "Same-size import completed.",
                pakPath,
                record.FileName,
                record.Offset,
                record.Size,
                backupPath);
        }
        catch (Exception ex)
        {
            result = new PakEditResult(
                false,
                "Same-size import failed: " + ex.Message,
                pakPath,
                record.FileName,
                record.Offset,
                record.Size,
                backupPath);
        }

        PakEditDiagnostics.AppendResult("same-size-update", result);
        return result;
    }

    public static PakEditResult ValidateSameSizeUpdate(string pakPath, IdxRecord record, int replacementSize)
    {
        if (!record.CanExtract)
        {
            return Fail("Record is not extractable.", pakPath, record);
        }

        if (!File.Exists(pakPath))
        {
            return Fail("PAK file does not exist.", pakPath, record);
        }

        if (record.Compression != 0 || record.CompressedSize is > 0)
        {
            return Fail("Compressed records cannot be updated by same-size raw import yet.", pakPath, record);
        }

        if (record.Offset < 0 || record.Size <= 0)
        {
            return Fail("Record offset/size is invalid.", pakPath, record);
        }

        if (replacementSize != record.Size)
        {
            return Fail($"Replacement size mismatch. Required {record.Size:N0} bytes, got {replacementSize:N0} bytes.", pakPath, record);
        }

        var pakSize = new FileInfo(pakPath).Length;
        if (record.Offset + record.Size > pakSize)
        {
            return Fail("Record range exceeds PAK file size.", pakPath, record);
        }

        return new PakEditResult(
            true,
            "Same-size update validation passed.",
            pakPath,
            record.FileName,
            record.Offset,
            record.Size);
    }

    private static string CreateBackup(string pakPath)
    {
        var directory = Path.GetDirectoryName(pakPath) ?? string.Empty;
        var fileName = Path.GetFileName(pakPath);
        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupPath = Path.Combine(directory, fileName + "." + stamp + ".bak");
        File.Copy(pakPath, backupPath, overwrite: false);
        return backupPath;
    }

    private static PakEditResult Fail(string message, string pakPath, IdxRecord record)
    {
        return new PakEditResult(
            false,
            message,
            pakPath,
            record.FileName,
            record.Offset,
            record.Size);
    }
}
