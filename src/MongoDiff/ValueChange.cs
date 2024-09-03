using MongoDB.Bson;

namespace MongoDiff;

public record ValueChange(string Path, object? OldValue, object? NewValue)
{
    public static IEnumerable<ValueChange> FromSimpleValue(string path, BsonValue left, BsonValue right)
    {
        var lValue = BsonTypeMapper.MapToDotNetValue(left);
        var rValue = BsonTypeMapper.MapToDotNetValue(right);
        if (!object.Equals(lValue, rValue))
        {
            yield return new ValueChange(path, lValue, rValue);
        }
    }

    public static IEnumerable<ValueChange> FromDocument(string path, BsonValue left, BsonValue right)
    {
        if (left.IsBsonNull)
        {
            return [new ValueChange(path, null, right.AsBsonDocument)];
        }
        if (right.IsBsonNull)
        {
            return [new ValueChange(path, left.AsBsonDocument, null)];
        }

        var diff = EntityDiff.Build(left.AsBsonDocument, right.AsBsonDocument);
        return diff.Changes.Select(change => change with { Path = $"{path}.{change.Path}" });
    }

    public static IEnumerable<ValueChange> FromArray(string path, BsonValue left, BsonValue right)
    {
        if (left.IsBsonNull)
        {
            yield return new ValueChange(path, null, right.AsBsonArray.Values);
        }
        else if (right.IsBsonNull)
        {
            yield return new ValueChange(path, left.AsBsonArray.Values, null);
        }
        else if (!left.AsBsonArray.SequenceEqual(right.AsBsonArray))
        {
            yield return new ValueChange(path, left.AsBsonArray.Values, right.AsBsonArray.Values);
        }
    }
}
