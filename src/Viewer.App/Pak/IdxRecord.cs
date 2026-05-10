namespace Viewer.App.Pak;

public sealed record IdxRecord(
    int Index,
    string FileName,
    int Size,
    int Offset,
    bool CanExtract = false,
    string Format = "unknown"
);
