using FluentAssertions;

using MongoDB.Bson;
using MongoDB.Bson.Serialization;

using Xunit;

namespace Larcanum.MongoDiff.UnitTests;

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
    public void EntityDiff_WithIdChange_ContainsAllChanges()
    {
        var entity = BsonSerializer.Deserialize<Employee>(File.ReadAllText("./test-data/employee.bson"));
        var left = entity.ToBsonDocument();
        entity.Id = ObjectId.GenerateNewId();
        var right = entity.ToBsonDocument();
        var diff = EntityDiff.Build(left, right);

        diff.Changes.Should().HaveCount(1);
        var change = diff.Changes.Single();
        change.OldValue.Should().Be(ObjectId.Parse("66c7091b4b5d3018e3cda214"));
        change.NewValue.Should().Be(entity.Id);
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

        var branchesChange = diff.Changes.Single(c => c.Path == "branches");
        branchesChange.OldValue.Should().BeAssignableTo<IEnumerable<BsonValue>>()
            .Which.Should().BeEmpty();
        branchesChange.NewValue.Should().BeAssignableTo<IEnumerable<BsonValue>>()
            .Which.Select(v => v.AsString).Should().BeEquivalentTo(entity.Branches);

        var dateOfBirthChange = diff.Changes.Single(c => c.Path == "dateOfBirth");
        dateOfBirthChange.OldValue.Should().Be(BsonNull.Value);
        dateOfBirthChange.NewValue.Should().Be("1980-01-01");

        var lastnameChange = diff.Changes.Single(c => c.Path == "lastname");
        lastnameChange.OldValue.Should().Be("Angerer");
        lastnameChange.NewValue.Should().Be("Osteron");

        var teamsChange = diff.Changes.Single(c => c.Path == "teams");
        teamsChange.OldValue.Should().BeAssignableTo<IEnumerable<BsonValue>>()
            .Which.Select(v => v.AsString).Should().BeEquivalentTo("Alpha");
        teamsChange.NewValue.Should().BeAssignableTo<IEnumerable<BsonValue>>()
            .Which.Select(v => v.AsString).Should().BeEquivalentTo(entity.Teams);
    }
}
