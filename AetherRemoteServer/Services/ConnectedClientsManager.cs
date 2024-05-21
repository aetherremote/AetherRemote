using AetherRemoteServer.Domain;

namespace AetherRemoteServer.Services;

/// <summary>
/// Manages all <see cref="ConnectedClient"/> and provides extension methods to access them
/// </summary>
public class ConnectedClientsManager
{
    /// <summary>
    /// Maps FriendCode to ConnectedClient
    /// </summary>
    private readonly Dictionary<string, ConnectedClient> connectedClients = [];

    /// <summary>
    /// Checks if provided friend code is already registered.
    /// </summary>
    /// <returns>Registration Status.</returns>
    public bool IsFriendCodeRegistered(string friendCode)
    {
        return connectedClients.ContainsKey(friendCode);
    }

    /// <summary>
    /// Creates a new <see cref="ConnectedClient"/> and adds it to the list of connected clients.
    /// </summary>
    /// <returns>The <see cref="ConnectedClient"/> created.</returns> 
    public ConnectedClient CreateClient(string connectionId, UserData data)
    {
        var connectedClient = new ConnectedClient(connectionId, data);
        connectedClients.Add(data.FriendCode, connectedClient);
        return connectedClient;
    }

    /// <summary>
    /// Attempts to get a <see cref="ConnectedClient"/> from list of connected clients.
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns><see cref="ConnectedClient"/> or null if not found.</returns>
    public ConnectedClient? GetConnectedClient(string friendCode)
    {
        connectedClients.TryGetValue(friendCode, out var client);
        return client;
    }

    /// <summary>
    /// Attempts to get a <see cref="ConnectedClient"/> by secret from list of connected clients.
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns><see cref="ConnectedClient"/> or null if not found.</returns>
    public ConnectedClient? GetConnectedClientBySecret(string secret)
    {
        return connectedClients.Values.FirstOrDefault(client => client.Data.Secret == secret);
    }

    /// <summary>
    /// Attempts to get a <see cref="ConnectedClient"/> by connection id from list of connected clients.
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns><see cref="ConnectedClient"/> or null if not found.</returns>
    public ConnectedClient? GetConnectedClientByConnectionId(string connectionId)
    {
        return connectedClients.Values.FirstOrDefault(client => client.ConnectionId == connectionId);
    }

    /// <summary>
    /// Attempts to remove a <see cref="ConnectedClient"/> from list of connected clients.
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns>True of successful, false otherwise.</returns>
    public bool RemoveConnectedClient(string friendCode)
    {
        return connectedClients.Remove(friendCode);
    }
}
