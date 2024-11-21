using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public interface IDiffProvider
{
    IEnumerable<ValueChange> FromSimpleValue(PropItem prop);
    IEnumerable<ValueChange> FromDocument(PropItem prop);
    IEnumerable<ValueChange> FromArray(PropItem prop);
}

public static class DiffProviderExtensions
{
    public static IEnumerable<ValueChange> FromDocument(this IDiffProvider diffProvider, Path path, BsonDocument left, BsonDocument right)
    {
        return diffProvider.FromDocument(new PropItem(path, new PropItemConfig(), BsonType.Document, left, right));
    }
}
