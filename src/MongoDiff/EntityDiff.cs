using MongoDB.Bson;

namespace MongoDiff;

public class EntityDiff
{
    private static readonly List<BsonType> IgnoredTypes = [BsonType.Null, BsonType.Binary, BsonType.EndOfDocument, BsonType.JavaScript, BsonType.JavaScriptWithScope, BsonType.MaxKey, BsonType.MinKey];

    public static EntityDiff Build(BsonDocument previous, BsonDocument current)
    {
        // TODO:
        // - deal with arrays
        // - deal with objects (recursion)
        // - deal with child entity (with ID) collections (path + ID + index as pseudo-property)
        // - allow _excluding_ properties

        var changes = DetermineProps(previous, current)
            .SelectMany(item => item.Type switch
            {
                BsonType.Array => ValueChange.FromArray(item.Prop, item.Left, item.Right),
                BsonType.Document => ValueChange.FromDocument(item.Prop, item.Left, item.Right),
                _ => ValueChange.FromSimpleValue(item.Prop, item.Left, item.Right)
            })
            .ToList();

        return new EntityDiff(changes);
    }

    private static IEnumerable<Item> DetermineProps(BsonDocument previous, BsonDocument current)
    {
        var left = previous.Elements.ToDictionary(e => e.Name, e => e.Value);
        var right = current.Elements.ToDictionary(e => e.Name, e => e.Value);

        foreach (var name in left.Keys.Union(right.Keys))
        {
            var leftVal = left.GetValueOrDefault(name, BsonNull.Value);
            var rightVal = right.GetValueOrDefault(name, BsonNull.Value);

            var mergedType = MergeTypes(leftVal.BsonType, rightVal.BsonType);
            if (IgnoredTypes.Contains(mergedType))
            {
                continue;
            }

            yield return new Item(name, mergedType, leftVal, rightVal);
        }
    }

    private static BsonType MergeTypes(BsonType left, BsonType right)
    {
        if (IsNullish(left) && IsNullish(right))
        {
            return BsonType.Null;
        }

        if (IsNullish(left))
        {
            return right;
        }

        if (IsNullish(right))
        {
            return left;
        }

        if (left != right)
        {
            throw new Exception($"Unexpected type mismatch between {left} and {right}");
        }

        return left;
    }

    private static bool IsNullish(BsonType type)
    {
        return type == BsonType.Null || type == BsonType.Undefined;
    }

    public IList<ValueChange> Changes { get; }

    private EntityDiff(IList<ValueChange> changes)
    {
        Changes = changes;
    }

    private record Item(string Prop, BsonType Type, BsonValue Left, BsonValue Right);
}
