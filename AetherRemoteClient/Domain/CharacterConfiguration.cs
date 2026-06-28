namespace AetherRemoteClient.Domain;

public class CharacterConfiguration(long id, string name, string world, long? secretId)
{
    public readonly long Id = id;
    public readonly string Name = name;
    public readonly string World = world;
    public long? SecretId = secretId;
}