using AetherRemoteCommon.Domain.CommonFriend;
using System.Collections.Generic;
using System.Linq;

namespace AetherRemoteClient.Domain;

public class FriendList()
{
    /// <summary>
    /// Local list of <see cref="Friend"></see>
    /// </summary>
    public List<Friend> Friends = [];

    /// <summary>
    /// Creates a <see cref="Friend"/> and adds them to the friend list
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns>Friend created</returns>
    public bool Add(string friendCode, bool online = false)
    {
        var exists = Friends.Any(existingFriend => existingFriend.FriendCode == friendCode);
        if (exists == false)
        {
            var newFriend = new Friend(friendCode);
            newFriend.Online = online;
            Friends.Add(newFriend);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes a friend from the friend list
    /// </summary>
    /// <param name="friendCode"></param>
    public void Remove(string friendCode)
    {
        Friends.RemoveAll(friend => friend.FriendCode == friendCode);
    }

    /// <summary>
    /// Tries to find friend with provided friend code
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns>Friend if found</returns>
    public Friend? Find(string friendCode)
    {
        return Friends.FirstOrDefault(friend => friend.FriendCode == friendCode);
    }
}
