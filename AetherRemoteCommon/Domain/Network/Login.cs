using AetherRemoteCommon.Domain.CommonFriend;

namespace AetherRemoteCommon.Domain.Network.Login;

public struct LoginRequest
{
    public string Secret { get; set; }

    public LoginRequest(string secret)
    {
        Secret = secret;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginRequest");
        sb.AddVariable("Secret", Secret);
        return sb.ToString();
    }
}

public struct LoginResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string FriendCode { get; set; }
    public List<Friend> FriendList { get; set; }

    public LoginResponse(bool success, string message, string friendCode, List<Friend>? friendList = null)
    {
        Success = success;
        Message = message;
        FriendCode = friendCode;
        FriendList = friendList ?? new();
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("FriendList", FriendList);
        return sb.ToString();
    }
}
