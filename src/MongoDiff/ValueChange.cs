using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public record ValueChange(string Path, BsonValue OldValue, BsonValue NewValue);
