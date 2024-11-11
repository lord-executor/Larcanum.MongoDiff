namespace Larcanum.MongoDiff;

public record RootSegment : IPathSegment
{
    public static RootSegment Root { get; } = new RootSegment();

    public bool IsRoot => true;
    public string Identifier => "$";

    private RootSegment() { }

    public override string ToString() => Identifier;
}
