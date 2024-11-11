using FluentAssertions;

using MongoDB.Bson;

using Xunit;
using Xunit.Abstractions;

namespace Larcanum.MongoDiff.UnitTests;

public class ChildEntityArrayTest
{
    public ChildEntityArrayTest()
    {
        MongoDBConfig.Initialize();
    }

    [Fact]
    public void EntityDiff_WithEmptyArrays_CreateNoChange()
    {
        var parent = new ParentEntity()
        {
            Id = ObjectId.GenerateNewId(),
            Date = DateOnly.FromDateTime(DateTime.Today)
        };

        var diff = EntityDiff.Build(parent.ToBsonDocument(), parent.ToBsonDocument());
        diff.Changes.Should().BeEmpty();
    }

    [Fact]
    public void EntityDiff_SameChildren_CreateNoChange()
    {
        var parent = new ParentEntity()
        {
            Id = ObjectId.GenerateNewId(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            Children = [CreateChildEntity("First"), CreateChildEntity("Second")]
        };

        var diff = EntityDiff.Build(parent.ToBsonDocument(), parent.ToBsonDocument());
        diff.Changes.Should().BeEmpty();
    }

    [Fact]
    public void EntityDiff_ChildWithDifferentId_CreatesReplacement()
    {
        var second = CreateChildEntity("Second");
        var parent = new ParentEntity()
        {
            Id = ObjectId.GenerateNewId(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            Children = [CreateChildEntity("First"), second]
        };
        var left = parent.ToBsonDocument();
        second.Id = ObjectId.GenerateNewId();

        var changes = EntityDiff.Build(left, parent.ToBsonDocument()).Changes;
        changes.Should().HaveCount(2);
    }

    [Fact]
    public void EntityDiff_ChildChanges_CreatesArrayValueChange()
    {
        var second = CreateChildEntity("Second");
        var parent = new ParentEntity()
        {
            Id = ObjectId.GenerateNewId(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            Children = [CreateChildEntity("First"), second]
        };
        var left = parent.ToBsonDocument();
        second.Name = "Updated";

        var changes = EntityDiff.Build(left, parent.ToBsonDocument()).Changes;
        changes.Should().HaveCount(1);
        var nameChange = changes.Single();
        nameChange.Path.Should().Be($"children[@{second.Id}].name");
        nameChange.OldValue.Should().Be("Second");
        nameChange.NewValue.Should().Be("Updated");
    }

    [Fact]
    public void EntityDiff_ReorderingOfChildren_CreatesNoChange()
    {
        var first = CreateChildEntity("First");
        var second = CreateChildEntity("Second");
        var parent = new ParentEntity()
        {
            Id = ObjectId.GenerateNewId(),
            Date = DateOnly.FromDateTime(DateTime.Today),
            Children = [first, second]
        };
        var left = parent.ToBsonDocument();
        parent.Children = [second, first];

        var changes = EntityDiff.Build(left, parent.ToBsonDocument()).Changes;
        changes.Should().BeEmpty();
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
