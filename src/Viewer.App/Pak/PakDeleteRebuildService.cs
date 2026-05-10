namespace Viewer.App.Pak;

public sealed record PakDeleteRebuildResult(
    bool Success,
    string Message,
    PakDeletePlan DeletePlan,
    PakRebuildResult RebuildResult,
    IdxWriteResult IdxWriteResult
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "PAK Delete Rebuild Result",
            "=========================",
            $"Success     : {Success}",
            $"Message     : {Message}",
            string.Empty,
            DeletePlan.ToDisplayText(),
            string.Empty,
            RebuildResult.ToDisplayText(),
            string.Empty,
            IdxWriteResult.ToDisplayText());
    }
}

public static class PakDeleteRebuildService
{
    public static PakDeleteRebuildResult RebuildWithoutSelectedRecords(
        string idxPath,
        IReadOnlyList<IdxRecord> allRecords,
        IReadOnlyList<IdxRecord> selectedRecords,
        bool overwriteOutputs = false)
    {
        var pakPath = PakExtractor.ResolvePakPath(idxPath);
        var deletePlan = PakDeletePlanner.BuildPlan(pakPath, selectedRecords);
        var recordsToDelete = selectedRecords
            .Where(selected => deletePlan.Items.Any(item => item.CanDelete && Matches(item, selected)))
            .ToList();

        if (recordsToDelete.Count == 0)
        {
            return BuildFailure(
                "No selected records can be deleted safely.",
                deletePlan,
                pakPath,
                IdxWriter.GetDefaultRebuiltIdxPath(idxPath));
        }

        var rebuild = PakRebuilder.RebuildWithoutRecords(
            pakPath,
            allRecords,
            recordsToDelete,
            outputPakPath: null,
            overwrite: overwriteOutputs);

        if (!rebuild.Success)
        {
            var failed = new PakDeleteRebuildResult(
                false,
                "PAK rebuild failed. IDX was not written.",
                deletePlan,
                rebuild,
                new IdxWriteResult(false, "IDX write skipped because PAK rebuild failed.", IdxWriter.GetDefaultRebuiltIdxPath(idxPath), 0, 0));
            WriteDiagnostics(failed);
            return failed;
        }

        var idxWrite = IdxWriter.WriteClassic28ForRebuild(idxPath, rebuild, overwriteOutputs);
        var success = idxWrite.Success;
        var message = success
            ? "Delete rebuild completed. Rebuilt PAK/IDX files were created."
            : "PAK rebuild completed, but rebuilt IDX write failed.";

        var result = new PakDeleteRebuildResult(success, message, deletePlan, rebuild, idxWrite);
        WriteDiagnostics(result);
        return result;
    }

    private static PakDeleteRebuildResult BuildFailure(string message, PakDeletePlan deletePlan, string pakPath, string outputIdxPath)
    {
        var rebuild = new PakRebuildResult(
            false,
            message,
            pakPath,
            PakRebuilder.GetDefaultOutputPath(pakPath),
            Array.Empty<PakRebuildRecord>(),
            Array.Empty<IdxRecord>(),
            File.Exists(pakPath) ? new FileInfo(pakPath).Length : 0,
            0);
        var idxWrite = new IdxWriteResult(false, "IDX write skipped.", outputIdxPath, 0, 0);
        var result = new PakDeleteRebuildResult(false, message, deletePlan, rebuild, idxWrite);
        WriteDiagnostics(result);
        return result;
    }

    private static bool Matches(PakDeletePlanItem item, IdxRecord record)
    {
        return item.Index == record.Index
            && item.FileName.Equals(record.FileName, StringComparison.OrdinalIgnoreCase)
            && item.Offset == record.Offset
            && item.Size == record.Size;
    }

    private static void WriteDiagnostics(PakDeleteRebuildResult result)
    {
        try
        {
            PakEditDiagnostics.Append(
                result.DeletePlan.PakPath,
                new PakEditDiagnosticEntry(
                    DateTime.Now,
                    "delete-rebuild",
                    result.Success,
                    result.DeletePlan.PakPath,
                    null,
                    result.ToDisplayText()));
        }
        catch
        {
            // Diagnostics must not break rebuild flow.
        }
    }
}
