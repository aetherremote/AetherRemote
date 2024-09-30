namespace AetherRemoteCommon.Domain.Network;

public struct GetUserRequest
{
    public string FriendCode { get; set; }

    public GetUserRequest(string friendCode)
    {
        FriendCode = friendCode;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("GetUserRequest");
        sb.AddVariable("FriendCode", FriendCode);
        return sb.ToString();
    }
}

public struct GetUserResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? FriendCode { get; set; }
    public string? Secret { get; set; }
    public bool? IsAdmin { get; set; }

    public GetUserResponse(bool success, string message = "", string? friendCode = null, string? secret = null, bool? isAdmin = null)
    {
        Success = success;
        Message = message;
        FriendCode = friendCode;
        Secret = secret;
        IsAdmin = isAdmin;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("GetUserRequest");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("IsAdmin", IsAdmin);
        return sb.ToString();
    }
}
