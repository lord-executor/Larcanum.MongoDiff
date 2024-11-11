using FluentAssertions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Xunit;
using Xunit.Abstractions;

namespace Larcanum.MongoDiff.UnitTests;

public class ComplexStructureTests
{
    private readonly ITestOutputHelper _output;

    public ComplexStructureTests(ITestOutputHelper output)
    {
        _output = output;
        MongoDBConfig.Initialize();
    }

    // [Fact]
    // public void EntityDiff_ComplexObjectChanges_CreatesExpectedDiff()
    // {
    //     var root = new TreeNode("Root Node",
    //         new TreeNode("Left Node", null, new TreeNode("Right of Left Node")),
    //         new TreeNode("Right Node"));
    //     var parent = new AllInOneEntity()
    //     {
    //         Id = ObjectId.GenerateNewId(),
    //         Node = root,
    //         Next = new AllInOneEntity
    //         {
    //             Id = ObjectId.GenerateNewId(),
    //             Tags = ["next"],
    //             Node = root.Right
    //         }
    //     };
    //
    //     var sourceDoc = parent.ToBsonDocument();
    //     _output.WriteLine(sourceDoc.ToString());
    //
    //     root.Right!.Right = root.Left!.Right;
    //     root.Left.Name = "No longer Left";
    //     root.Left.Left = new TreeNode("Left of no longer left");
    //     parent.Next!.Node!.Right!.Name = "Two places at once";
    //
    //     var targetDoc = parent.ToBsonDocument();
    //     _output.WriteLine(targetDoc.ToString());
    //
    //     var diff = EntityDiff.Build(sourceDoc, targetDoc);
    //     _output.WriteLine(diff.ToBsonDocument().ToString());
    // }

    [Theory]
    [InlineData("test-data/source-01.bson", "test-data/target-01.bson", "test-data/diff-01.bson")]
    [InlineData("test-data/source-02.bson", "test-data/target-02.bson", "test-data/diff-02.bson")]
    [InlineData("test-data/source-03.bson", "test-data/target-03.bson", "test-data/diff-03.bson")]
    public void EntityDiff_WithKnownSourceAndTarget_CreatesExpectedDiff(string source, string target, string diff)
    {
        var sourceDoc = BsonSerializer.Deserialize<AllInOneEntity>(File.ReadAllText(source));
        var targetDoc = BsonSerializer.Deserialize<AllInOneEntity>(File.ReadAllText(target));
        var expectedDiff = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(diff));

        var entityDiff = EntityDiff.Build(sourceDoc.ToBsonDocument(), targetDoc.ToBsonDocument());
        expectedDiff.Equals(entityDiff.ToBsonDocument()).Should().BeTrue();
    }

    private ChildEntity CreateChildEntity(string name)
    {
        return new ChildEntity()
        {
            Id = ObjectId.GenerateNewId(),
            Age = Random.Shared.Next(1, 100),
            Name = name,
        };
    }
}
