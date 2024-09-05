using MongoDB.Bson;

namespace MongoDiff.UnitTests;

public class ParentEntity
{
    public ObjectId Id { get; set; }
    public DateOnly Date { get; set; }
    public IList<ChildEntity> Children { get; set; } = new List<ChildEntity>();
}
