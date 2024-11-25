using FluentAssertions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Xunit;

namespace Larcanum.MongoDiff.UnitTests;

public class EntityDiffApplyTest
{
    public EntityDiffApplyTest()
    {
        MongoDBConfig.Initialize();
    }

    [Theory]
    [InlineData("test-data/source-01.bson", "test-data/target-01.bson", "test-data/diff-01.bson")]
    [InlineData("test-data/source-02.bson", "test-data/target-02.bson", "test-data/diff-02.bson")]
    [InlineData("test-data/source-03.bson", "test-data/target-03.bson", "test-data/diff-03.bson")]
    public void ApplyingDiff_ToSource_CreatesTarget(string source, string target, string diff)
    {
        var sourceDoc = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(source));
        var targetDoc = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(target));
        var entityDiff = BsonSerializer.Deserialize<EntityDiff>(File.ReadAllText(diff));

        entityDiff.ApplyTo(sourceDoc);
        sourceDoc.ToString().Should().Be(targetDoc.ToString());
    }

    [Theory]
    [InlineData("test-data/source-01.bson", "test-data/target-01.bson", "test-data/diff-01.bson")]
    [InlineData("test-data/source-02.bson", "test-data/target-02.bson", "test-data/diff-02.bson")]
    [InlineData("test-data/source-03.bson", "test-data/target-03.bson", "test-data/diff-03.bson")]
    public void RevertingDiff_ToTarget_CreatesSource(string source, string target, string diff)
    {
        var sourceDoc = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(source));
        var targetDoc = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText(target));
        var entityDiff = BsonSerializer.Deserialize<EntityDiff>(File.ReadAllText(diff));

        entityDiff.RevertFrom(targetDoc);
        targetDoc.ToString().Should().Be(sourceDoc.ToString());
    }
}
