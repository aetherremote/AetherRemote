namespace AetherRemoteCommon.Domain.Network;

public struct CreateOrUpdateUserRequest
{
    public string FriendCode { get; set; }
    public string Secret { get; set; }
    public bool IsAdmin { get; set; }

    public CreateOrUpdateUserRequest(string friendCode, string secret, bool isAdmin = false)
    {
        FriendCode = friendCode;
        Secret = secret;
        IsAdmin = isAdmin;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("CreateOrUpdateUserRequest");
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Secret", Secret);
        sb.AddVariable("IsAdmin", IsAdmin);
        return sb.ToString();
    }
}

public struct CreateOrUpdateUserResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public CreateOrUpdateUserResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("CreateOrUpdateUserResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
