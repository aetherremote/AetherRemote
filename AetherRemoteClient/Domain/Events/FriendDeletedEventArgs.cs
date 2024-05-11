using System;

namespace AetherRemoteClient.Domain.Events;

public class FriendDeletedEventArgs(string friendCode) : EventArgs 
{
    /// <summary>
    /// The friend who was deleted.
    /// </summary>
    public string FriendCode = friendCode;
}
