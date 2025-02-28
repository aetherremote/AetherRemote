using System;

namespace AetherRemoteClient.Domain.Events;

/// <summary>
///     Event data for when a friend is deleted from the friend's list
/// </summary>
public class FriendDeletedEventArgs(Friend friend) : EventArgs
{
    /// <summary>
    ///     The deleted friend
    /// </summary>
    public readonly Friend Friend = friend;
}