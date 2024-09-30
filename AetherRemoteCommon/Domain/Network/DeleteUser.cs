namespace AetherRemoteCommon.Domain.Network;

public struct DeleteUserRequest
{
    public string FriendCode { get; set; }

    public DeleteUserRequest(string friendCode)
    {
        FriendCode = friendCode;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DeleteUserRequest");
        sb.AddVariable("FriendCode", FriendCode);
        return sb.ToString();
    }
}

public struct DeleteUserResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public DeleteUserResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DeleteUserResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
