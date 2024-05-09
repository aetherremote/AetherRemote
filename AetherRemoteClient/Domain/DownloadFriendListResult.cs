using AetherRemoteClient.Providers;
using AetherRemoteCommon.Domain.CommonFriend;
using System.Collections.Generic;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Object returned when calling <see cref="NetworkProvider.DownloadFriendList(string)"/>
/// </summary>
public class DownloadFriendListResult(bool success, string message, List<Friend> friends)
{
    public bool Success = success;
    public string Message = message;
    public List<Friend> Friends = friends;
}
