namespace Larcanum.MongoDiff;

public interface IPathSegment
{
    bool IsRoot { get; }
    string Identifier { get; }
}
