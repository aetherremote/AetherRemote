namespace AetherRemoteClient.Domain;

/// <summary>
///     Represents the current character of the client
/// </summary>
/// <param name="name">Character's Name</param>
/// <param name="world">Character's World</param>
public class LocalCharacter(string name, string world)
{
    public readonly string Name = name;
    public readonly string World = world;
    public readonly string FullName = string.Concat(name, "@", world);
}