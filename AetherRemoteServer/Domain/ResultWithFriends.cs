using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteServer.Services;

namespace AetherRemoteServer.Domain;

/// <summary>
/// Used to give <see cref="NetworkService.FetchFriendList"/> a strongly typed return object
/// </summary>
public class ResultWithFriends
{
    public bool Success;
    public string Message;
    public List<Friend> FriendList;

    public ResultWithFriends(bool success, string message = "", List<Friend>? friendList = null)
    {
        Success = success;
        Message = message;
        FriendList = friendList ?? [];
    }
}
