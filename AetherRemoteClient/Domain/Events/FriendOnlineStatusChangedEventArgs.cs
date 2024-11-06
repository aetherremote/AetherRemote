using System;

namespace AetherRemoteClient.Domain.Events;

/// <summary>
/// Event containing the details of which friend came online or offline
/// </summary>
public class FriendOnlineStatusChangedEventArgs(Friend friend, bool online) : EventArgs
{
    /// <summary>
    /// Friend whose online status changed
    /// </summary>
    public readonly Friend Friend = friend;
    
    /// <summary>
    /// Is said friend online or offline
    /// </summary>
    public readonly bool Online = online;
}