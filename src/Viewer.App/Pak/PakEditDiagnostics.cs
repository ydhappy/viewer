namespace Viewer.App.Pak;

public sealed record PakEditDiagnosticEntry(
    DateTime Timestamp,
    string Operation,
    bool Success,
    string PakPath,
    string? RecordName,
    string Message,
    string? BackupPath = null
)
{
    public string ToLogLine()
    {
        return string.Join("\t", new[]
        {
            Timestamp.ToString("yyyy-MM-dd HH:mm:ss"),
            Operation,
            Success ? "SUCCESS" : "FAIL",
            PakPath,
            RecordName ?? string.Empty,
            BackupPath ?? string.Empty,
            Message.ReplaceLineEndings(" ")
        });
    }
}

public static class PakEditDiagnostics
{
    public static string GetDefaultLogPath(string pakPath)
    {
        var directory = Path.GetDirectoryName(pakPath) ?? string.Empty;
        var fileName = Path.GetFileName(pakPath);
        return Path.Combine(directory, fileName + ".edit-log.txt");
    }

    public static void Append(string pakPath, PakEditDiagnosticEntry entry)
    {
        var logPath = GetDefaultLogPath(pakPath);
        var shouldWriteHeader = !File.Exists(logPath);
        using var writer = new StreamWriter(logPath, append: true, System.Text.Encoding.UTF8);
        if (shouldWriteHeader)
        {
            writer.WriteLine("Timestamp\tOperation\tStatus\tPakPath\tRecordName\tBackupPath\tMessage");
        }

        writer.WriteLine(entry.ToLogLine());
    }

    public static void AppendResult(string operation, PakEditResult result)
    {
        Append(result.PakPath, new PakEditDiagnosticEntry(
            DateTime.Now,
            operation,
            result.Success,
            result.PakPath,
            result.FileName,
            result.Message,
            result.BackupPath));
    }

    public static void AppendBackup(string operation, PakBackupInfo backup, bool success, string message)
    {
        Append(backup.PakPath, new PakEditDiagnosticEntry(
            DateTime.Now,
            operation,
            success,
            backup.PakPath,
            null,
            message,
            backup.BackupPath));
    }

    public static void AppendFailure(string pakPath, string operation, string message, string? recordName = null, string? backupPath = null)
    {
        Append(pakPath, new PakEditDiagnosticEntry(
            DateTime.Now,
            operation,
            false,
            pakPath,
            recordName,
            message,
            backupPath));
    }
}
