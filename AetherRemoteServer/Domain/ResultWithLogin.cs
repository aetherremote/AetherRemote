using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteServer.Domain;

public class ResultWithLogin
{
    public bool Success;
    public string Message;
    public string FriendCode;
    public List<Friend> FriendList;

    public ResultWithLogin(bool success, string message = "", string friendCode = "", List<Friend>? friendList = null)
    {
        Success = success;
        Message = message;
        FriendCode = friendCode;
        FriendList = friendList ?? [];
    }
}
