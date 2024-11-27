using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Larcanum.MongoDiff;

public record ValueChange(string Path, BsonValue OldValue, BsonValue NewValue, [property: BsonIgnoreIfDefault] int? OldIndex = null, [property: BsonIgnoreIfDefault] int? NewIndex = null);
