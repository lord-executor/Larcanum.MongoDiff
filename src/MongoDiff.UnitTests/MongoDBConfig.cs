using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;

namespace MongoDiff.UnitTests;

public static class MongoDBConfig
{
    private static bool _initialized = false;

    public static void Initialize()
    {
        if (!_initialized)
        {
            _initialized = true;
            var pack = new ConventionPack();
            pack.Add(new CamelCaseElementNameConvention());
            ConventionRegistry.Register("Camel Case Convention", pack, t => true);

            BsonSerializer.RegisterSerializer(new DateOnlySerializer());
            BsonSerializer.RegisterSerializer(new TimeOnlySerializer());
        }
    }
}
