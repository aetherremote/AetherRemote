namespace AetherRemoteCommon.Domain.Network;

public struct LoginDetailsRequest
{
    public LoginDetailsRequest() { }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginDetailsRequest");
        return sb.ToString();
    }
}

public struct LoginDetailsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? FriendCode { get; set; }
    public Dictionary<string, UserPermissions>? Permissions { get; set; }
    public HashSet<string>? Online { get; set; }

    public LoginDetailsResponse(
        bool success, string? 
        friendCode = null, 
        Dictionary<string, UserPermissions>? permissions = null,
        HashSet<string>? online = null,
        string message = "")
    {
        Success = success;
        Message = message;
        FriendCode = friendCode;
        Permissions = permissions;
        Online = online;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginDetailsResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("Permissions", Permissions);
        sb.AddVariable("Online", Online);
        return sb.ToString();
    }
}
