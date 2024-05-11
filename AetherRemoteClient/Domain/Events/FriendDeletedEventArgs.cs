using System;

namespace AetherRemoteClient.Domain.Events;

public class FriendDeletedEventArgs(string friendCode) : EventArgs 
{
    /// <summary>
    /// The friend code of the deleted friend.
    /// </summary>
    public string FriendCode = friendCode;
}
