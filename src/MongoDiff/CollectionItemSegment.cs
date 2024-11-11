namespace Larcanum.MongoDiff;

public record CollectionItemSegment(string Identifier) : IPathSegment
{
    public bool IsRoot => false;

    public override string ToString() => $"[@{Identifier}]";
}
