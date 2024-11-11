using MongoDB.Bson;

namespace Larcanum.MongoDiff.UnitTests;

public class ChildEntity
{
    public ObjectId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
