using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public static class BsonTypeExtensions
{
    public static BsonType MergeTypes(BsonType left, BsonType right)
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

    public static bool IsNullish(this BsonType type)
    {
        return type is BsonType.Null or BsonType.Undefined;
    }
}
