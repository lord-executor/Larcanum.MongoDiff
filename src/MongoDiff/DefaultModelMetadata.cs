namespace Larcanum.MongoDiff;

public class DefaultModelMetadata : IModelMetadata
{
    public PropItemConfig ConfigFor(Path path)
    {
        return new PropItemConfig(false, new ObjectIdKeyTransform());
    }
}
