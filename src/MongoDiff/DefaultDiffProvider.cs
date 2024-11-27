using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public partial class DefaultDiffProvider : IDiffProvider
{
    private readonly IModelMetadata _modelMetadata;

    public DefaultDiffProvider(IModelMetadata modelMetadata)
    {
        _modelMetadata = modelMetadata;
    }

    public IEnumerable<ValueChange> FromSimpleValue(PropItem prop)
    {
        var lValue = BsonTypeMapper.MapToDotNetValue(prop.Left);
        var rValue = BsonTypeMapper.MapToDotNetValue(prop.Right);
        if (!Equals(lValue, rValue))
        {
            yield return new ValueChange(prop.Path.FullPath(), prop.Left, prop.Right);
        }
    }

    public IEnumerable<ValueChange> FromDocument(PropItem prop)
    {
        if (prop.Left.IsBsonNull)
        {
            return [new ValueChange(prop.Path.FullPath(), BsonNull.Value, prop.Right.AsBsonDocument)];
        }
        if (prop.Right.IsBsonNull)
        {
            return [new ValueChange(prop.Path.FullPath(), prop.Left.AsBsonDocument, BsonNull.Value)];
        }

        return DetermineProps(prop.Path, prop.Left.AsBsonDocument, prop.Right.AsBsonDocument)
            .Where(item => !item.Config.Ignore)
            .SelectMany(item => item.Type switch
            {
                BsonType.Array => FromArray(item),
                BsonType.Document => FromDocument(item),
                _ => FromSimpleValue(item)
            });
    }

    public IEnumerable<ValueChange> FromArray(PropItem prop)
    {
        if (prop.Left.IsBsonNull)
        {
            return [new ValueChange(prop.Path.FullPath(), BsonNull.Value, prop.Right.AsBsonArray)];
        }
        if (prop.Right.IsBsonNull)
        {
            return [new ValueChange(prop.Path.FullPath(), prop.Left.AsBsonArray, BsonNull.Value)];
        }

        var item = prop.Left.AsBsonArray.Concat(prop.Right.AsBsonArray).FirstOrDefault();
        if (item != null && item.IsBsonDocument && prop.Config.Key != null)
        {
            var changes = new List<ValueChange>();
            var leftMap = prop.Left.AsBsonArray.Select(x => x.AsBsonDocument).ToDictionary(prop.Config.Key.ExtractKey);
            var rightMap = prop.Right.AsBsonArray.Select(x => x.AsBsonDocument).ToDictionary(prop.Config.Key.ExtractKey);
            var keys = leftMap.Keys.Union(rightMap.Keys).Order();
            foreach (var id in keys)
            {
                var itemPath = prop.Path.Append(new CollectionItemSegment(id.ToString()!));
                if (leftMap.TryGetValue(id, out var leftDoc) && rightMap.TryGetValue(id, out var rightDoc))
                {
                    changes.AddRange(this.FromDocument(itemPath, leftDoc, rightDoc));
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

        if (!prop.Left.AsBsonArray.SequenceEqual(prop.Right.AsBsonArray))
        {
            var changes = new List<ValueChange>();
            var leftMap = prop.Left.AsBsonArray.Select(x => x.AsBsonDocument).Index().ToDictionary(x => x.Index, x => x.Item);
            var rightMap = prop.Right.AsBsonArray.Select(x => x.AsBsonDocument).Index().ToDictionary(x => x.Index, x => x.Item);
            var keys = leftMap.Keys.Union(rightMap.Keys).Order();
            foreach (var id in keys)
            {
                var itemPath = prop.Path.Append(new CollectionItemSegment(id.ToString()!));
                if (leftMap.TryGetValue(id, out var leftDoc) && rightMap.TryGetValue(id, out var rightDoc))
                {
                    changes.AddRange(this.FromDocument(itemPath, leftDoc, rightDoc));
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
