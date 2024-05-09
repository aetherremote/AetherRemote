using AetherRemoteCommon.Domain.CommonFriend;
using Dalamud.Interface;
using System.Collections.Generic;
using System.Linq;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Represents a session used in the Session Tab
/// </summary>
public class Session(string id, FontAwesomeIcon icon, string? name = null)
{
    /// <summary>
    /// Session Id
    /// </summary>
    public string Id = id;

    /// <summary>
    /// Name of the session.
    /// </summary>
    public string Name = name ?? id;

    /// <summary>
    /// The icon for the session
    /// </summary>
    public FontAwesomeIcon Icon = icon;

    /// <summary>
    /// List of friends locked into the session
    /// </summary>
    public List<Friend> TargetFriends = [];

    public string TargetFriendsAsList()
    {
        return string.Join(", ", TargetFriends.Select(friend => friend.NoteOrFriendCode));
    }
}
