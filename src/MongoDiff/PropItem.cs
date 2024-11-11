using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public record PropItem(Path Prop, PropItemConfig Config, BsonType Type, BsonValue Left, BsonValue Right);
