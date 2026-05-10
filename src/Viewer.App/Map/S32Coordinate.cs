namespace Viewer.App.Map;

public sealed record S32Coordinate(
    int X,
    int Y,
    string Source
)
{
    public override string ToString()
    {
        return $"{X},{Y} ({Source})";
    }
}
