using MongoDB.Bson;

namespace MongoDiff;

public record ValueChange(string Path, object? OldValue, object? NewValue)
{
    public static ValueChange? FromSimpleValue(string path, BsonValue left, BsonValue right)
    {
        var lValue = BsonTypeMapper.MapToDotNetValue(left);
        var rValue = BsonTypeMapper.MapToDotNetValue(right);
        if (object.Equals(lValue, rValue))
        {
            return null;
        }
        else
        {
            return new ValueChange(path, lValue, rValue);
        }
    }

    public static ValueChange? FromArray(string path, BsonValue left, BsonValue right)
    {

        if (left.IsBsonArray && right.IsBsonArray && left.AsBsonArray.SequenceEqual(right.AsBsonArray))
        {
            return null;
        }
        else
        {
            return new ValueChange(path, left.IsBsonArray ? left.AsBsonArray.Values : null, right.IsBsonArray ? right.AsBsonArray.Values : null);
        }
    }
}
