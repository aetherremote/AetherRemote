using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Saves;
using Dalamud.Plugin;
using System.Collections.Generic;
using System.Linq;

namespace AetherRemoteClient.Providers;

public class FriendListProvider(DalamudPluginInterface pluginInterface)
{
    private const string FileName = "friends.json";
    private readonly SaveFile<FriendListSave> saveSystem = new(pluginInterface.ConfigDirectory.FullName, FileName);
    private readonly List<Friend> devFriends =
    [
        new Friend("Demo1"),
        new Friend("Demo2"),
        new Friend("Demo3", "Nickname!"),
        new Friend("Demo4"),
        new Friend("Demo5"),
        new Friend("Demo6", "Catch Phrase!")
    ];

    public List<Friend> FriendList => Plugin.DeveloperMode ? devFriends : saveSystem.Get.Friends;

    public void Save()
    {
        saveSystem.Save();
    }

    /// <summary>
    /// Adds a friend code to the frined list. Checks if friend code already exists before doing so. 
    /// </summary>
    /// <param name="friendCode"></param>
    /// <returns>The newly created friend, otherwise null.</returns>
    public Friend? AddFriend(string friendCode)
    {
        var friend = FriendList.FirstOrDefault(fr => fr.FriendCode == friendCode);
        if (friend == null)
        {
            friend = new Friend(friendCode);
            FriendList.Add(friend);
        }

        return friend;
    }

    /// <summary>
    /// Remove friend by friend code
    /// </summary>
    public bool RemoveFriend(string friendCode)
    {
        var index = FriendList.FindIndex(friend => friend.FriendCode == friendCode);
        if (index < 0)
            return false;

        FriendList.RemoveAt(index);
        return true;
    }

    /// <summary>
    /// Replaces the current friend list
    /// </summary>
    public void ReplaceFriendList(List<Friend> newFriendList)
    {
        FriendList.Clear();
        FriendList.AddRange(newFriendList);
    }
}
