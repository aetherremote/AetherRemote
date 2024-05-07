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
    /// Represents a client that is connected and logged into the server
    /// </summary>
    public ConnectedClient(string connectionId, UserData data)
    {
        ConnectionId = connectionId;
        Data = data;
    }
}
