using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public partial class DefaultDiffProvider : IDiffProvider
{
    private readonly IModelMetadata _modelMetadata;

    public DefaultDiffProvider(IModelMetadata modelMetadata)
    {
        _modelMetadata = modelMetadata;
    }

    public IEnumerable<ValueChange> FromSimpleValue(Path path, BsonValue left, BsonValue right)
    {
        var lValue = BsonTypeMapper.MapToDotNetValue(left);
        var rValue = BsonTypeMapper.MapToDotNetValue(right);
        if (!Equals(lValue, rValue))
        {
            yield return new ValueChange(path.FullPath(), left, right);
        }
    }

    public IEnumerable<ValueChange> FromDocument(Path path, BsonValue left, BsonValue right)
    {
        if (left.IsBsonNull)
        {
            return [new ValueChange(path.FullPath(), BsonNull.Value, right.AsBsonDocument)];
        }
        if (right.IsBsonNull)
        {
            return [new ValueChange(path.FullPath(), left.AsBsonDocument, BsonNull.Value)];
        }

        return DetermineProps(path, left.AsBsonDocument, right.AsBsonDocument)
            .Where(item => !item.Config.Ignore)
            .SelectMany(item => item.Type switch
            {
                BsonType.Array => FromArray(item.Prop, item.Left, item.Right),
                BsonType.Document => FromDocument(item.Prop, item.Left, item.Right),
                _ => FromSimpleValue(item.Prop, item.Left, item.Right)
            });
    }

    public IEnumerable<ValueChange> FromArray(Path path, BsonValue left, BsonValue right)
    {
        if (left.IsBsonNull)
        {
            return [new ValueChange(path.FullPath(), BsonNull.Value, right.AsBsonArray)];
        }
        if (right.IsBsonNull)
        {
            return [new ValueChange(path.FullPath(), left.AsBsonArray, BsonNull.Value)];
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
                Path itemPath = path.Append(new CollectionItemSegment(id.ToString()!));
                if (leftMap.TryGetValue(id, out var leftDoc) && rightMap.TryGetValue(id, out var rightDoc))
                {
                    changes.AddRange(FromDocument(itemPath, leftDoc, rightDoc));
                }
                else if (leftMap.TryGetValue(id, out var value))
                {
                    changes.Add(new ValueChange(itemPath.FullPath(), value, BsonNull.Value));
                }
                else
                {
                    changes.Add(new ValueChange(itemPath.FullPath(), BsonNull.Value, rightMap[id]));
                }
            }
            return changes;
        }

        if (!left.AsBsonArray.SequenceEqual(right.AsBsonArray))
        {
            return [new ValueChange(path.FullPath(), left.AsBsonArray, right.AsBsonArray)];
        }

        return [];
    }

    private IEnumerable<PropItem> DetermineProps(Path path, BsonDocument previous, BsonDocument current)
    {
        var left = previous.Elements.ToDictionary(e => e.Name, e => e.Value);
        var right = current.Elements.ToDictionary(e => e.Name, e => e.Value);

        foreach (var name in left.Keys.Union(right.Keys))
        {
            var childPath = path.Append(new PropertySegment(name));
            var config = _modelMetadata.ConfigFor(childPath);
            var leftVal = left.GetValueOrDefault(name, BsonNull.Value);
            var rightVal = right.GetValueOrDefault(name, BsonNull.Value);

            var mergedType = BsonTypeExtensions.MergeTypes(leftVal.BsonType, rightVal.BsonType);
            if (IgnoredTypes.Contains(mergedType))
            {
                continue;
            }

            yield return new PropItem(childPath, config, mergedType, leftVal, rightVal);
        }
    }

    private static readonly List<BsonType> IgnoredTypes = [BsonType.Null, BsonType.Binary, BsonType.EndOfDocument, BsonType.JavaScript, BsonType.JavaScriptWithScope, BsonType.MaxKey, BsonType.MinKey];
}
