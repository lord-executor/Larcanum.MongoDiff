using MongoDB.Bson;

namespace MongoDiff.UnitTests;

public class AllInOneEntity
{
    public ObjectId Id { get; set; }
    public HashSet<string> Tags { get; set; } = new HashSet<string>();
    public BsonDocument? Doc { get; set; }
    public ParentEntity? Parent { get; set; }
    public TreeNode? Node { get; set; }
    public AllInOneEntity? Next { get; set; }
}
