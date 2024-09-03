using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace MongoDiff.UnitTests;

/// <summary>
/// Serialized as an ISO 8601 time string with millisecond accuracy. Note that <see cref="TimeOnly"/> represents
/// a local wall-clock time and is thus context-sensitive.
/// </summary>
public class TimeOnlySerializer : SerializerBase<TimeOnly>
{
    public override TimeOnly Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
    {
        var type = context.Reader.GetCurrentBsonType();
        switch (type)
        {
            case BsonType.String:
                return TimeOnly.ParseExact(context.Reader.ReadString(), "HH:mm:ss.fff");
            default:
                throw new NotSupportedException($"Cannot convert a {type} to a DateOnly.");
        }
    }
    public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TimeOnly value)
    {
        context.Writer.WriteString(value.ToString("HH:mm:ss.fff"));
    }
}
