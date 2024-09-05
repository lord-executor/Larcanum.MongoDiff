using MongoDB.Bson;

namespace MongoDiff.UnitTests;

public class ChildEntity
{
    public ObjectId Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Age { get; set; }
}
