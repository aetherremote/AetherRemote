using System;

namespace AetherRemoteClient.Domain.Events;

/// <summary>
/// Event containing the details of which friend was deleted
/// </summary>
public class FriendDeletedEventArgs(Friend friend) : EventArgs
{
    /// <summary>
    /// The friend who was deleted
    /// </summary>
    public readonly Friend Friend = friend;
}
