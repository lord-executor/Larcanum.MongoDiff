using FluentAssertions;

using MongoDB.Bson;

using Xunit;

namespace MongoDiff.UnitTests;

public class NestedObjectTest
{
    public NestedObjectTest()
    {
        MongoDBConfig.Initialize();
    }

    [Fact]
    public void EntityDiff_NestedObjectPropertyChange_HasCorrectPaths()
    {
        var root = new TreeNode("root",
            new TreeNode("left", new TreeNode("left-left"), new TreeNode("left-right")),
            new TreeNode("right", new TreeNode("right-left"), null));

        var before = root.ToBsonDocument();
        root.Left!.Name = "left-new";
        root.Left!.Left!.Name = "left-left-new";
        root.Right!.Left!.Name = "right-left-new";

        var diff = EntityDiff.Build(before, root.ToBsonDocument());
        diff.Changes.Should().HaveCount(3);
        var changes = diff.Changes.ToDictionary(c => c.Path, c => c);
        changes.Keys.Should().Contain(["left.name", "left.left.name", "right.left.name"]);

        changes["left.name"].OldValue.Should().Be("left");
        changes["left.name"].NewValue.Should().Be("left-new");

        changes["left.left.name"].OldValue.Should().Be("left-left");
        changes["left.left.name"].NewValue.Should().Be("left-left-new");

        changes["right.left.name"].OldValue.Should().Be("right-left");
        changes["right.left.name"].NewValue.Should().Be("right-left-new");
    }

    [Fact]
    public void EntityDiff_NestedObjectAdditionRemoval_HasCorrectPaths()
    {
        var root = new TreeNode("root",
            new TreeNode("left", new TreeNode("left-left"), null),
            new TreeNode("right", new TreeNode("right-left"), null));

        var before = root.ToBsonDocument();
        root.Left = null;
        root.Right!.Right = new TreeNode("right-right-new");

        var diff = EntityDiff.Build(before, root.ToBsonDocument());
        diff.Changes.Should().HaveCount(2);
        var changes = diff.Changes.ToDictionary(c => c.Path, c => c);
        changes.Keys.Should().Contain(["left", "right.right"]);


        changes["left"].OldValue.Should().BeOfType<BsonDocument>();
        changes["left"].OldValue!.ToString().Should().Be("""{ "name" : "left", "left" : { "name" : "left-left", "left" : null, "right" : null }, "right" : null }""");
        changes["left"].NewValue.Should().Be(BsonNull.Value);

        changes["right.right"].OldValue.Should().Be(BsonNull.Value);
        changes["right.right"].NewValue.Should().BeOfType<BsonDocument>();
        changes["right.right"].NewValue!.ToString().Should().Be("""{ "name" : "right-right-new", "left" : null, "right" : null }""");
    }
}
