using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public interface IKeyTransform
{
    string ExtractKey(BsonDocument doc);
    Predicate<BsonDocument> MatchKey(string key);
}
