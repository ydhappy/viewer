namespace Viewer.App.Pak;

public sealed record PakBackupInfo(
    string PakPath,
    string BackupPath,
    long Size,
    DateTime CreatedAt
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "PAK Backup",
            "==========",
            $"PAK    : {PakPath}",
            $"Backup : {BackupPath}",
            $"Size   : {Size:N0} bytes",
            $"Created: {CreatedAt:yyyy-MM-dd HH:mm:ss}");
    }
}

public static class PakBackupService
{
    public static PakBackupInfo CreateBackup(string pakPath, string? backupDirectory = null)
    {
        if (!File.Exists(pakPath))
        {
            throw new FileNotFoundException("PAK file does not exist.", pakPath);
        }

        var sourceInfo = new FileInfo(pakPath);
        var directory = string.IsNullOrWhiteSpace(backupDirectory)
            ? sourceInfo.DirectoryName ?? string.Empty
            : backupDirectory;
        Directory.CreateDirectory(directory);

        var stamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var backupName = sourceInfo.Name + "." + stamp + ".bak";
        var backupPath = Path.Combine(directory, backupName);
        File.Copy(pakPath, backupPath, overwrite: false);

        return new PakBackupInfo(pakPath, backupPath, sourceInfo.Length, DateTime.Now);
    }

    public static PakBackupInfo RestoreBackup(string pakPath, string backupPath, bool createPreRestoreBackup = true)
    {
        if (!File.Exists(backupPath))
        {
            throw new FileNotFoundException("Backup file does not exist.", backupPath);
        }

        if (File.Exists(pakPath) && createPreRestoreBackup)
        {
            CreateBackup(pakPath);
        }

        var backupInfo = new FileInfo(backupPath);
        File.Copy(backupPath, pakPath, overwrite: true);
        return new PakBackupInfo(pakPath, backupPath, backupInfo.Length, DateTime.Now);
    }

    public static IReadOnlyList<PakBackupInfo> FindBackups(string pakPath)
    {
        if (string.IsNullOrWhiteSpace(pakPath))
        {
            return Array.Empty<PakBackupInfo>();
        }

        var pakInfo = new FileInfo(pakPath);
        if (pakInfo.DirectoryName is null || !Directory.Exists(pakInfo.DirectoryName))
        {
            return Array.Empty<PakBackupInfo>();
        }

        var pattern = pakInfo.Name + ".*.bak";
        return Directory.EnumerateFiles(pakInfo.DirectoryName, pattern)
            .Select(path => new FileInfo(path))
            .OrderByDescending(info => info.LastWriteTime)
            .Select(info => new PakBackupInfo(pakPath, info.FullName, info.Length, info.LastWriteTime))
            .ToList();
    }
}
