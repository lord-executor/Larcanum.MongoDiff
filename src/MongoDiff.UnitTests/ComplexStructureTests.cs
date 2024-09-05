using FluentAssertions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Xunit;
using Xunit.Abstractions;

namespace MongoDiff.UnitTests;

public class ComplexStructureTests
{
    private readonly ITestOutputHelper _output;

    public ComplexStructureTests(ITestOutputHelper output)
    {
        _output = output;
        MongoDBConfig.Initialize();
    }

    [Fact]
    public void EntityDiff_ComplexObjectChanges_CreatesExpectedDiff()
    {
        var parent = new AllInOneEntity()
        {
            Id = ObjectId.GenerateNewId(),
            Tags = ["test", "sample2"],
            Doc = new ChildEntity() { Id = ObjectId.GenerateNewId(), Name = "Pure Doc" }.ToBsonDocument(),
            Parent = new ParentEntity() {
                Id = ObjectId.GenerateNewId(),
                Date = new DateOnly(2024, 8, 2),
                Children =
                [
                    CreateChildEntity("C1"),
                    CreateChildEntity("C2"),
                    CreateChildEntity("C3"),
                    CreateChildEntity("C4"),
                ]
            }
        };
        _output.WriteLine(parent.ToBsonDocument().ToString());
    }

    [Theory]
    [InlineData("test-data/source-01.bson", "test-data/target-01.bson")]
    public void GetDiff(string source, string target)
    {
        var sourceDoc = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(source));
        var targetDoc = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(target));

        var diff = EntityDiff.Build(sourceDoc, targetDoc);
        _output.WriteLine(diff.ToBsonDocument().ToString());
    }

    [Theory]
    [InlineData("test-data/source-01.bson", "test-data/target-01.bson", "test-data/diff-01.bson")]
    public void EntityDiff_WithKnownSourceAndTarget_CreatesExpectedDiff(string source, string target, string diff)
    {
        var sourceDoc = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(source));
        var targetDoc = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(target));
        var expectedDiff = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(diff));

        var entityDiff = EntityDiff.Build(sourceDoc, targetDoc);
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
