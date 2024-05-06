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
    public bool CreateAndAddFriend(string friendCode)
    {
        var exists = Friends.Any(existingFriend => existingFriend.FriendCode == friendCode);
        if (exists == false)
        {
            Friends.Add(new Friend(friendCode));
            return true;
        }

        return false;
    }

    /// <summary>
    /// Removes a friend from the friend list
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns>Success</returns>
    public bool RemoveFriend(string friendCode)
    {
        var index = Friends.FindIndex(friend => friend.FriendCode == friendCode);
        if (index < 0)
            return false;

        Friends.RemoveAt(index);
        return true;
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
