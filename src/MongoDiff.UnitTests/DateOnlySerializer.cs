using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Larcanum.MongoDiff.UnitTests;

/// <summary>
/// Serialized as an ISO 8601 string.
/// </summary>
public class DateOnlySerializer : SerializerBase<DateOnly>
{
    private const string StringFormat = "yyyy-MM-dd";

    public override DateOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        switch (type)
        {
            case BsonType.String:
                return DateOnly.ParseExact(context.Reader.ReadString(), StringFormat);
            default:
                throw new NotSupportedException($"Cannot convert a {type} to a DateOnly.");
        }
    }
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, DateOnly value)
    {
        context.Writer.WriteString(value.ToString(StringFormat));
    }
}
