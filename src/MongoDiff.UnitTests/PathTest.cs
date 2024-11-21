using FluentAssertions;

using Xunit;

namespace Larcanum.MongoDiff.UnitTests;

public class PathTest
{
    [Fact]
    public void RootPath_OnlyContains_RootSegment()
    {
        Path.Root.FullPath().Should().Be("$");
    }

    [Fact]
    public void NewPath_Contains_NoSegments()
    {
        new Path().FullPath().Should().Be("");
    }

    [Theory]
    [MemberData(nameof(ParseExamples))]
    public void Parse_StringPath_BuildsPathSegments(string path, IPathSegment[] segments)
    {
        Path.Parse(path).Should().ContainInOrder(segments);
    }

    [Fact]
    public void Parse_InvalidPath_ThrowsException()
    {
        Action action = () => Path.Parse("$.first.sec-ond[@0]");
            action.Should().ThrowExactly<FormatException>()
                .Which.Message.Should().Contain("Unmatched pattern at 11 ...-ond[@0]");
    }

    [Theory]
    [InlineData("$", "Test")]
    [InlineData("$.alpha.Beta", "Gamma")]
    [InlineData("$.List[@5]", "value")]
    public void AppendPropertySegment_ExistingPath_BuildsCorrectPath(string existingPath, string property)
    {
        Path.Parse(existingPath).Append(new PropertySegment(property)).FullPath()
            .Should().Be($"{existingPath}.{property}");
    }

    [Theory]
    [InlineData("$.Hello[@World]._id")]
    [InlineData("$.Item[@42].Nested[@foo]")]
    public void Parse_AndReconstruct_FullPathsRemainTheSame(string path)
    {
        var parsed = Path.Parse(path);
        var copy = parsed.Aggregate(new Path(), (p, s) => p.Append(s));
        parsed.FullPath().Should().Be(copy.FullPath());
    }

    public static IEnumerable<object[]> ParseExamples =>
        new List<object[]>
        {
            new object[] { "$", new IPathSegment[] { RootSegment.Root } },
            new object[] { "$.foo.bar", new IPathSegment[] { RootSegment.Root, new PropertySegment("foo"), new PropertySegment("bar") } },
            new object[] { "$[@he63kdh].Value", new IPathSegment[] { RootSegment.Root, new CollectionItemSegment("he63kdh"), new PropertySegment("Value") } },
            new object[] { "$[@673c6]._id", new IPathSegment[] { RootSegment.Root, new CollectionItemSegment("673c6"), new PropertySegment("_id") } },
            new object[] { ".relative", new IPathSegment[] { new PropertySegment("relative") } },
            new object[] { "[@relative-index]", new IPathSegment[] { new CollectionItemSegment("relative-index") } },
            new object[] { "$[@root][@nested].prop", new IPathSegment[] { RootSegment.Root, new CollectionItemSegment("root"), new CollectionItemSegment("nested"), new PropertySegment("prop") } },
        };
}
