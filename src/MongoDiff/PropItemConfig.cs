namespace Larcanum.MongoDiff;

public record PropItemConfig(bool Ignore = false, IKeyTransform? Key = null);
