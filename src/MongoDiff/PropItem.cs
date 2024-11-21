using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public record PropItem(Path Path, PropItemConfig Config, BsonType Type, BsonValue Left, BsonValue Right);
