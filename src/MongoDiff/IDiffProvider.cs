using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public interface IDiffProvider
{
    IEnumerable<ValueChange> FromSimpleValue(Path path, BsonValue left, BsonValue right);
    IEnumerable<ValueChange> FromDocument(Path path, BsonValue left, BsonValue right);
    IEnumerable<ValueChange> FromArray(Path path, BsonValue left, BsonValue right);
}
