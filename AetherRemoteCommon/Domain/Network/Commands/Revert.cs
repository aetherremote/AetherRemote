namespace AetherRemoteCommon.Domain.Network.Commands;

public struct RevertRequest
{
    public List<string> TargetFriendCodes { get; set; }
    public RevertType RevertType { get; set; }

    public RevertRequest(List<string> targetFriendCodes, RevertType revertType)
    {
        TargetFriendCodes = targetFriendCodes;
        RevertType = revertType;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("RevertRequest");
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("RevertType", RevertType);
        return sb.ToString();
    }
}

public struct RevertResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public RevertResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("RevertResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

public struct RevertCommand
{
    public string SenderFriendCode { get; set; }
    public RevertType RevertType { get; set; }

    public RevertCommand(string senderFriendCode, RevertType revertType)
    {
        SenderFriendCode = senderFriendCode;
        RevertType = revertType;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("RevertCommand");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("RevertType", RevertType);
        return sb.ToString();
    }
}
