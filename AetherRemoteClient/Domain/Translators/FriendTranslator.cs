using System.Collections.Generic;

using CommonFriend = AetherRemoteCommon.Domain.CommonFriend.Friend;

namespace AetherRemoteClient.Domain.Translators;

public static class FriendTranslator
{
    public static CommonFriend DomainToCommon(Friend friend)
    {
        return new CommonFriend(friend.FriendCode, friend.Note, friend.Permissions);
    }

    public static Friend CommonToDomain(CommonFriend friend)
    {
        return new Friend(friend.FriendCode, friend.Note, friend.Permissions);
    }

    public static List<CommonFriend> DomainFriendListToCommon(List<Friend> friends)
    {
        var converted = new List<CommonFriend>();
        foreach (var friend in friends)
        {
            converted.Add(DomainToCommon(friend));
        }
        return converted;
    }

    public static List<Friend> CommonFriendListToDomain(List<CommonFriend> friends)
    {
        var converted = new List<Friend>();
        foreach (var friend in friends)
        {
            converted.Add(CommonToDomain(friend));
        }
        return converted;
    }
}
