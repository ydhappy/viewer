using System.Text;

namespace Viewer.App.Pak;

public sealed record PakAddRebuildResult(
    bool Success,
    string Message,
    string SourceIdxPath,
    string SourcePakPath,
    string InputFilePath,
    string AddedRecordName,
    PakRebuildResult RebuildResult,
    IdxWriteResult IdxWriteResult
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "PAK Add Rebuild Result",
            "======================",
            $"Success     : {Success}",
            $"Message     : {Message}",
            $"Source IDX  : {SourceIdxPath}",
            $"Source PAK  : {SourcePakPath}",
            $"Input file  : {InputFilePath}",
            $"Record name : {AddedRecordName}",
            string.Empty,
            RebuildResult.ToDisplayText(),
            string.Empty,
            IdxWriteResult.ToDisplayText());
    }
}

public static class PakAddRebuildService
{
    private const int ClassicNameLength = 20;

    public static PakAddRebuildResult RebuildWithAddedFile(
        string idxPath,
        IReadOnlyList<IdxRecord> allRecords,
        string inputFilePath,
        string? recordName = null,
        bool overwriteOutputs = false)
    {
        var pakPath = PakExtractor.ResolvePakPath(idxPath);
        var outputIdxPath = IdxWriter.GetDefaultRebuiltIdxPath(idxPath);
        var outputPakPath = PakRebuilder.GetDefaultOutputPath(pakPath);
        var normalizedRecordName = NormalizeRecordName(recordName, inputFilePath);

        var validation = ValidateAddRequest(pakPath, allRecords, inputFilePath, normalizedRecordName, outputPakPath, outputIdxPath, overwriteOutputs);
        if (!validation.Success)
        {
            WriteDiagnostics(validation);
            return validation;
        }

        var rebuild = PakRebuilder.RebuildWithoutRecords(
            pakPath,
            allRecords,
            Array.Empty<IdxRecord>(),
            outputPakPath: outputPakPath,
            overwrite: overwriteOutputs);

        if (!rebuild.Success)
        {
            var failed = BuildResult(
                false,
                "Base PAK rebuild failed. Added file was not appended.",
                idxPath,
                pakPath,
                inputFilePath,
                normalizedRecordName,
                rebuild,
                new IdxWriteResult(false, "IDX write skipped because base PAK rebuild failed.", outputIdxPath, 0, 0));
            WriteDiagnostics(failed);
            return failed;
        }

        var addedRecord = AppendAddedFile(outputPakPath, inputFilePath, normalizedRecordName, allRecords.Count + 1);
        var combinedRecords = rebuild.Records
            .Select(record => record.ToIdxRecord())
            .Concat(new[] { addedRecord })
            .ToList();

        var finalRebuild = rebuild with
        {
            Records = rebuild.Records.Concat(new[] { new PakRebuildRecord(addedRecord, addedRecord.Offset, addedRecord.Size) }).ToList(),
            OutputSize = new FileInfo(outputPakPath).Length
        };

        var idxWrite = IdxWriter.WriteClassic28(outputIdxPath, combinedRecords, overwriteOutputs);
        var success = idxWrite.Success;
        var message = success
            ? "Add rebuild completed. Rebuilt PAK/IDX files were created."
            : "Added file was appended to rebuilt PAK, but rebuilt IDX write failed.";

        var result = BuildResult(success, message, idxPath, pakPath, inputFilePath, normalizedRecordName, finalRebuild, idxWrite);
        WriteDiagnostics(result);
        return result;
    }

    private static PakAddRebuildResult ValidateAddRequest(
        string pakPath,
        IReadOnlyList<IdxRecord> allRecords,
        string inputFilePath,
        string recordName,
        string outputPakPath,
        string outputIdxPath,
        bool overwriteOutputs)
    {
        var emptyRebuild = new PakRebuildResult(false, "Validation failed.", pakPath, outputPakPath, Array.Empty<PakRebuildRecord>(), Array.Empty<IdxRecord>(), File.Exists(pakPath) ? new FileInfo(pakPath).Length : 0, 0);
        var skippedIdx = new IdxWriteResult(false, "IDX write skipped.", outputIdxPath, 0, 0);

        if (!File.Exists(pakPath))
        {
            return BuildResult(false, "Source PAK file does not exist.", string.Empty, pakPath, inputFilePath, recordName, emptyRebuild, skippedIdx);
        }

        if (!File.Exists(inputFilePath))
        {
            return BuildResult(false, "Input file does not exist.", string.Empty, pakPath, inputFilePath, recordName, emptyRebuild, skippedIdx);
        }

        var inputSize = new FileInfo(inputFilePath).Length;
        if (inputSize > int.MaxValue)
        {
            return BuildResult(false, "Input file is too large for current IDX record model.", string.Empty, pakPath, inputFilePath, recordName, emptyRebuild, skippedIdx);
        }

        if (string.IsNullOrWhiteSpace(recordName))
        {
            return BuildResult(false, "Record name is empty.", string.Empty, pakPath, inputFilePath, recordName, emptyRebuild, skippedIdx);
        }

        if (Encoding.Default.GetBytes(recordName).Length > ClassicNameLength)
        {
            return BuildResult(false, $"Record name exceeds {ClassicNameLength} bytes for Classic28 IDX.", string.Empty, pakPath, inputFilePath, recordName, emptyRebuild, skippedIdx);
        }

        if (allRecords.Any(record => record.FileName.Equals(recordName, StringComparison.OrdinalIgnoreCase)))
        {
            return BuildResult(false, "Record name already exists. Add-as-new does not overwrite existing entries.", string.Empty, pakPath, inputFilePath, recordName, emptyRebuild, skippedIdx);
        }

        if (!overwriteOutputs && File.Exists(outputPakPath))
        {
            return BuildResult(false, "Output rebuilt PAK already exists.", string.Empty, pakPath, inputFilePath, recordName, emptyRebuild, skippedIdx);
        }

        if (!overwriteOutputs && File.Exists(outputIdxPath))
        {
            return BuildResult(false, "Output rebuilt IDX already exists.", string.Empty, pakPath, inputFilePath, recordName, emptyRebuild, skippedIdx);
        }

        return BuildResult(true, "Add request validation passed.", string.Empty, pakPath, inputFilePath, recordName, emptyRebuild, skippedIdx);
    }

    private static IdxRecord AppendAddedFile(string outputPakPath, string inputFilePath, string recordName, int index)
    {
        var inputBytes = File.ReadAllBytes(inputFilePath);
        using var output = new FileStream(outputPakPath, FileMode.Append, FileAccess.Write, FileShare.None);
        var offset = checked((int)output.Position);
        output.Write(inputBytes, 0, inputBytes.Length);
        output.Flush(flushToDisk: true);

        return new IdxRecord(
            Index: index,
            FileName: recordName,
            Size: inputBytes.Length,
            Offset: offset,
            CanExtract: true,
            Format: "classic-28");
    }

    private static string NormalizeRecordName(string? recordName, string inputFilePath)
    {
        return string.IsNullOrWhiteSpace(recordName)
            ? Path.GetFileName(inputFilePath)
            : recordName.Trim();
    }

    private static PakAddRebuildResult BuildResult(
        bool success,
        string message,
        string idxPath,
        string pakPath,
        string inputFilePath,
        string recordName,
        PakRebuildResult rebuildResult,
        IdxWriteResult idxWriteResult)
    {
        return new PakAddRebuildResult(success, message, idxPath, pakPath, inputFilePath, recordName, rebuildResult, idxWriteResult);
    }

    private static void WriteDiagnostics(PakAddRebuildResult result)
    {
        try
        {
            PakEditDiagnostics.Append(
                result.SourcePakPath,
                new PakEditDiagnosticEntry(
                    DateTime.Now,
                    "add-rebuild",
                    result.Success,
                    result.SourcePakPath,
                    result.AddedRecordName,
                    result.ToDisplayText()));
        }
        catch
        {
            // Diagnostics must not break rebuild flow.
        }
    }
}
