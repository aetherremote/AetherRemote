using System.Collections.Generic;
using System.Linq;

using CommonFriend = AetherRemoteCommon.Domain.CommonFriend.Friend;

namespace AetherRemoteClient.Domain;

public class FriendList
{
    public readonly List<Friend> Friends;

    public FriendList(List<CommonFriend> friends)
    {
        Friends = friends.Select(friend => new Friend(friend.FriendCode, friend.Note, friend.Permissions)).ToList();
    }

    /// <summary>
    /// Creates a <see cref="Friend"/> and adds them to the friend list
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns>Friend created</returns>
    public bool CreateAndAddFriend(string friendCode, bool online = false)
    {
        var exists = Friends.Any(existingFriend => existingFriend.FriendCode == friendCode);
        if (exists == false)
        {
            Friends.Add(new Friend(friendCode) { Online = online });
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes a friend from the friend list
    /// </summary>
    /// <param name="friendCode"></param>
    public void RemoveFriend(string friendCode)
    {
        Friends.RemoveAll(friend => friend.FriendCode == friendCode);
    }

    /// <summary>
    /// Converts Friend List into Common Friend List
    /// </summary>
    /// <returns></returns>
    public List<CommonFriend> Convert()
    {
        return Friends.Select(friend => friend.Convert()).ToList();
    }

    /// <summary>
    /// Tries to find friend with provided friend code
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns>Friend if found</returns>
    public Friend? FindFriend(string friendCode)
    {
        return Friends.FirstOrDefault(friend => friend.FriendCode == friendCode);
    }
}
