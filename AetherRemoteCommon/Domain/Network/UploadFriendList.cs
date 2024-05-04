using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteCommon.Domain.Network.UploadFriendList;

public struct UploadFriendListRequest
{
    public string Secret { get; set; }
    public List<Friend> FriendList { get; set; }

    public UploadFriendListRequest(string secrert, List<Friend> friendList)
    {
        Secret = secrert;
        FriendList = friendList;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("UploadFriendListRequest");
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("FriendList", FriendList);
        return sb.ToString();
    }
}

public struct UploadFriendListResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public UploadFriendListResponse(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("UploadFriendListResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
