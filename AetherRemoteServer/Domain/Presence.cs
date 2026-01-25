namespace AetherRemoteServer.Domain;

public class Presence(string connectionId, string characterName, string characterWorld)
{
    public readonly string ConnectionId = connectionId;
    public readonly string CharacterName = characterName;
    public readonly string CharacterWorld = characterWorld;

    /// <summary>
    ///     Token bucket used for general features
    /// </summary>
    public readonly TokenBucket GeneralBucket = new(2);
    
    /// <summary>
    ///     Token bucket used for possession features
    /// </summary>
    public readonly TokenBucket PossessionBucket = new(10);
}