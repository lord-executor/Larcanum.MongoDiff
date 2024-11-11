namespace Larcanum.MongoDiff;

public record PropertySegment(string Identifier) : IPathSegment
{
    public bool IsRoot => false;

    public override string ToString() => $".{Identifier}";
}
