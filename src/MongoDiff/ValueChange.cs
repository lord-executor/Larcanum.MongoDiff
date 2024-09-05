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
            return [new ValueChange(path, null, right.AsBsonArray.Values)];
        }
        if (right.IsBsonNull)
        {
            return [new ValueChange(path, left.AsBsonArray.Values, null)];
        }

        var item = left.AsBsonArray.Concat(right.AsBsonArray).FirstOrDefault();
        if (item != null && item.IsBsonDocument && item.AsBsonDocument.Contains("_id") && item.AsBsonDocument["_id"].IsObjectId)
        {
            var changes = new List<ValueChange>();
            var leftMap = left.AsBsonArray.Select(x => x.AsBsonDocument).ToDictionary(x => x["_id"]);
            var rightMap = right.AsBsonArray.Select(x => x.AsBsonDocument).ToDictionary(x => x["_id"]);
            var keys = leftMap.Keys.Union(rightMap.Keys).Order();
            foreach (var id in keys)
            {
                if (leftMap.TryGetValue(id, out var leftDoc) && rightMap.TryGetValue(id, out var rightDoc))
                {
                    changes.AddRange(EntityDiff.Build(leftDoc, rightDoc).Changes.Select(change => change with { Path = $"{path}[@{id}].{change.Path}" }));
                }
                else if (leftMap.TryGetValue(id, out var value))
                {
                    changes.Add(new ValueChange($"{path}[@{id}]", value, null));
                }
                else
                {
                    changes.Add(new ValueChange($"{path}[@{id}]", null, rightMap[id]));
                }
            }
            return changes;
        }

        if (!left.AsBsonArray.SequenceEqual(right.AsBsonArray))
        {
            return [new ValueChange(path, left.AsBsonArray.Values, right.AsBsonArray.Values)];
        }

        return [];
    }
}
