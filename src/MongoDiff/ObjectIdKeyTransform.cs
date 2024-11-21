using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public class ObjectIdKeyTransform : IKeyTransform
{
    private readonly string _key = "_id";

    public ObjectIdKeyTransform() {}

    public ObjectIdKeyTransform(string key)
    {
        _key = key;
    }

    public string ExtractKey(BsonDocument doc)
    {
        return doc[_key].AsObjectId.ToString();
    }

    public Predicate<BsonDocument> MatchKey(string key)
    {
        var id = ObjectId.Parse(key);
        return doc => doc[_key].AsObjectId == id;
    }
}
