using AetherRemoteCommon.Domain.CommonGlamourerApplyType;

namespace AetherRemoteCommon.Domain.Network.Commands;

public struct TransformRequest
{
    public List<string> TargetFriendCodes { get; set; }
    public string GlamourerData { get; set; }
    public GlamourerApplyFlag ApplyType { get; set; }

    public TransformRequest(List<string> targetFriendCodes, string glamourerData, GlamourerApplyFlag applyType)
    {
        TargetFriendCodes = targetFriendCodes;
        GlamourerData = glamourerData;
        ApplyType = applyType;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("TransformRequest");
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("GlamourerData", GlamourerData);
        sb.AddVariable("ApplyType", ApplyType);
        return sb.ToString();
    }

}

public struct TransformResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public TransformResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("TransformResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

public struct TransformCommand
{
    public string SenderFriendCode { get; set; }
    public string GlamourerData { get; set; }
    public GlamourerApplyFlag ApplyFlags { get; set; }

    public TransformCommand(string senderFriendCode, string glamourerData, GlamourerApplyFlag applyFlags)
    {
        SenderFriendCode = senderFriendCode;
        GlamourerData = glamourerData;
        ApplyFlags = applyFlags;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("TransformCommand");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("GlamourerData", GlamourerData);
        sb.AddVariable("ApplyFlags", ApplyFlags);
        return sb.ToString();
    }
}
