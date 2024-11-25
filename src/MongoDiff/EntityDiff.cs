using MongoDB.Bson;

namespace Larcanum.MongoDiff;

public class EntityDiff
{
    public static EntityDiff Build(BsonDocument previous, BsonDocument current)
    {
        var modelMetadata = new DefaultModelMetadata();
        var provider = new DefaultDiffProvider(modelMetadata);

        return new EntityDiff(provider.FromDocument(Path.Root, previous, current).ToList());
    }

    public IList<ValueChange> Changes { get; }

    public EntityDiff(IList<ValueChange> changes)
    {
        Changes = changes;
    }

    public void ApplyTo(BsonDocument document)
    {
        var modelMetadata = new DefaultModelMetadata();

        foreach (var change in Changes)
        {
            SetValue(modelMetadata, Path.Parse(change.Path), document, change.NewValue);
        }
    }

    public void RevertFrom(BsonDocument document)
    {
        var modelMetadata = new DefaultModelMetadata();

        foreach (var change in Changes)
        {
            SetValue(modelMetadata, Path.Parse(change.Path), document, change.OldValue);
        }
    }

    private void SetValue(IModelMetadata metadata, Path path, BsonValue target, BsonValue value)
    {
        var segments = path.ToList();
        var current = target;
        for (var i = 0; i < segments.Count; i++)
        {
            if (i == segments.Count - 1)
            {
                _ = segments[i] switch
                {
                    RootSegment _ => throw new InvalidOperationException("Cannot set root"),
                    PropertySegment prop => current.AsBsonDocument[prop.Identifier] = value,
                    CollectionItemSegment item => SetArrayValue(current.AsBsonArray, value, IndexByKey(current.AsBsonArray, metadata.ConfigFor(new Path(segments[..i])), item.Identifier)),
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
            else
            {
                current = segments[i] switch
                {
                    RootSegment _ => target,
                    PropertySegment prop => current.AsBsonDocument[prop.Identifier],
                    CollectionItemSegment item => current.AsBsonArray[IndexByKey(current.AsBsonArray, metadata.ConfigFor(new Path(segments[..i])), item.Identifier)],
                    _ => throw new ArgumentOutOfRangeException()
                };
            }
        }
    }

    private static int IndexByKey(BsonArray array, PropItemConfig config, string key)
    {
        var keyPredicate = (config.Key ?? throw new NullReferenceException("PropItemConfig.Key cannot be null when resolving collection items by key"))
            .MatchKey(key);

        return array.Index().FirstOrDefault(x => keyPredicate(x.Item.AsBsonDocument), (-1, BsonNull.Value)).Index;
    }

    private static BsonValue SetArrayValue(BsonArray array, BsonValue value, int index)
    {
        if (index < 0)
        {
            array.Add(value);
        }
        else if (value == BsonNull.Value)
        {
            array.RemoveAt(index);
        }
        else
        {
            array[index] = value;
        }

        return value;
    }
}
