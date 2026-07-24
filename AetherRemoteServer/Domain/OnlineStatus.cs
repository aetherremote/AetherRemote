namespace AetherRemoteServer.Domain;

public enum OnlineStatus
{
    /// <summary> The user is disconnected, but only recently, and may be reconnecting </summary>
    Disconnected,
    
    /// <summary> The user is connected </summary>
    Online
}