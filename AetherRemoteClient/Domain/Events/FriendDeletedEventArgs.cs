using System;

namespace AetherRemoteClient.Domain.Events;

public class FriendDeletedEventArgs(Friend friend) : EventArgs
{
    /// <summary>
    /// The friend who was deleted
    /// </summary>
    public Friend Friend = friend;
}
