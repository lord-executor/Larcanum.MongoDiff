using FluentAssertions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Xunit;

namespace MongoDiff.UnitTests;

public class SimpleObjectTest
{
    public SimpleObjectTest()
    {
        MongoDBConfig.Initialize();
    }

    [Fact]
    public void EntityDiff_OfSameDocument_IsEmpty()
    {
        var doc = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText("./test-data/employee.bson"));
        EntityDiff.Build(doc, doc).Changes.Should().BeEmpty();
    }

    [Fact]
    public void EntityDiff_OfSameSource_IsEmpty()
    {
        var docLeft = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText("./test-data/employee.bson"));
        var docRight = BsonSerializer.Deserialize<BsonDocument>(File.ReadAllText("./test-data/employee.bson"));
        EntityDiff.Build(docLeft, docRight).Changes.Should().BeEmpty();
    }

    [Fact]
    public void EntityDiff_WithMultipleChanges_ContainsAllChanges()
    {
        var entity = BsonSerializer.Deserialize<Employee>(File.ReadAllText("./test-data/employee.bson"));
        var left = entity.ToBsonDocument();
        entity.Lastname = "Osteron";
        entity.Teams.Add("Test");
        entity.Branches = ["main", "feature"];
        entity.DateOfBirth = new DateOnly(1980, 1, 1);
        var right = entity.ToBsonDocument();
        var diff = EntityDiff.Build(left, right);

        diff.Changes.Should().HaveCount(4);
        diff.Changes.Select(c => c.Path).Order().Should()
            .ContainInOrder(["branches", "dateOfBirth", "lastname", "teams"]);
    }
}
