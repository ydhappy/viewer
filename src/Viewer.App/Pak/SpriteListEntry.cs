namespace Viewer.App.Pak;

public sealed record SpriteListEntry(
    int Index,
    int SpriteId,
    string Name,
    string Group,
    string Action,
    int Frame
)
{
    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "Sprite List Entry",
            "=================",
            $"Index    : {Index}",
            $"Sprite ID: {SpriteId}",
            $"Name     : {Name}",
            $"Group    : {Group}",
            $"Action   : {Action}",
            $"Frame    : {Frame}");
    }
}

public sealed class SpriteListCatalog
{
    private readonly Dictionary<int, SpriteListEntry> _bySpriteId;
    private readonly Dictionary<string, SpriteListEntry> _byName;

    public SpriteListCatalog(string filePath, IReadOnlyList<SpriteListEntry> entries)
    {
        FilePath = filePath;
        Entries = entries;
        _bySpriteId = entries
            .GroupBy(entry => entry.SpriteId)
            .ToDictionary(group => group.Key, group => group.First());
        _byName = entries
            .Where(entry => !string.IsNullOrWhiteSpace(entry.Name))
            .GroupBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => group.First(), StringComparer.OrdinalIgnoreCase);
    }

    public string FilePath { get; }

    public IReadOnlyList<SpriteListEntry> Entries { get; }

    public SpriteListEntry? FindBySpriteId(int spriteId)
    {
        return _bySpriteId.TryGetValue(spriteId, out var entry) ? entry : null;
    }

    public SpriteListEntry? FindByName(string name)
    {
        return _byName.TryGetValue(name, out var entry) ? entry : null;
    }

    public string ToDisplayText()
    {
        return string.Join(Environment.NewLine,
            "Sprite List Catalog",
            "===================",
            $"File   : {FilePath}",
            $"Entries: {Entries.Count:N0}",
            string.Empty,
            "※ 현재 단계는 list.spr 파싱/매핑 준비입니다.",
            "※ 실제 SPR 프레임 렌더링은 후속 단계에서 연결합니다.");
    }
}
