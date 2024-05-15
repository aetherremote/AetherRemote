namespace AetherRemoteServer.Domain;

/// <summary>
/// Represents a client that is connected and logged into the server
/// </summary>
public class ConnectedClient
{
    /// <summary>
    /// The connection id of this client
    /// </summary>
    public string ConnectionId { get; set; }

    /// <summary>
    /// The <see cref="UserData"/> of this client
    /// </summary>
    public UserData Data { get; set; }

    /// <summary>
    /// The last time this client sent a command
    /// </summary>
    public DateTime LastCommandTimestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Represents a client that is connected and logged into the server
    /// </summary>
    public ConnectedClient(string connectionId, UserData data)
    {
        ConnectionId = connectionId;
        Data = data;
    }

    /// <summary>
    /// Checks to see if a <see cref="ConnectedClient"/> is friends with another.
    /// </summary>
    public bool IsFriendsWith(ConnectedClient client)
    {
        return Data.FriendList.Any(friend => friend.FriendCode == client.Data.FriendCode);
    }
}
