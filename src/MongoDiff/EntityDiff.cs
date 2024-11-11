using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public class EntityDiff
{
    public static EntityDiff Build(BsonDocument previous, BsonDocument current)
    {
        // TODO:
        // - deal with arrays
        // - deal with objects (recursion)
        // - deal with child entity (with ID) collections (path + ID + index as pseudo-property)
        // - allow _excluding_ properties

        var modelMetadata = new DefaultModelMetadata();
        var provider = new DefaultDiffProvider(modelMetadata);

        return new EntityDiff(provider.FromDocument(Path.Root, previous, current).ToList());
    }

    public IList<ValueChange> Changes { get; }

    public EntityDiff(IList<ValueChange> changes)
    {
        Changes = changes;
    }
}
