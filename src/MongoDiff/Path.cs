using System.Text.RegularExpressions;

namespace Larcanum.MongoDiff;

public partial class Path
{
    [GeneratedRegex(@"\G(\$)|\G\.(\w+)|\G\[@([^\]]+?)\]")]
    private static partial Regex SegmentExpression();

    public static Path Root { get; } = new Path([RootSegment.Root]);

    public static Path Parse(string path)
    {
        var matchEnd = 0;
        var segments = new List<IPathSegment>();
        foreach (Match m in SegmentExpression().Matches(path))
        {
            if (m.Groups[1].Success)
            {
                segments.Add(RootSegment.Root);
            }
            else if (m.Groups[2].Success)
            {
                segments.Add(new PropertySegment(m.Groups[2].Value));
            }
            else if (m.Groups[3].Success)
            {
                segments.Add(new CollectionItemSegment(m.Groups[3].Value));
            }
            matchEnd = m.Index + m.Length;
        }

        if (matchEnd < path.Length)
        {
            throw new FormatException($"Invalid path. Unmatched pattern at {matchEnd} ...{path[Range.StartAt(matchEnd)]}");
        }

        return new Path(segments);
    }

    private readonly IList<IPathSegment> _segments;

    public Path()
        : this(new List<IPathSegment>())
    {
    }

    private Path(IEnumerable<IPathSegment> segments)
    {
        _segments = segments.ToList();
    }

    public Path Append(IPathSegment segment)
    {
        return new Path(_segments.Append(segment));
    }

    public string FullPath() => string.Join(string.Empty, _segments);
}
