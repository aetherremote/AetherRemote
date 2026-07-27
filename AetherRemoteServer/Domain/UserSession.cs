namespace AetherRemoteServer.Domain;

/// <summary>
///     Represents a connected session
/// </summary>
public class UserSession(string connectionId, string characterName, string characterWorld)
{
    /// <summary> The SignalR connection string </summary>
    public string ConnectionId = connectionId;
    
    /// <summary> This user's in-game character name </summary>
    public readonly string CharacterName = characterName;
    
    /// <summary> This user's in-game character world </summary>
    public readonly string CharacterWorld = characterWorld;
    
    /// <summary> This user's connectivity status as known by the server </summary>
    public OnlineStatus OnlineStatus = OnlineStatus.Online;
    
    /// <summary> This user's token bucket, which contains tokens that can be 'spent' on calling hub methods </summary>
    public readonly TokenBucket GeneralBucket = new(4, 0.5);
}