using AetherRemoteCommon.Domain.CommonChatMode;

namespace AetherRemoteCommon.Domain.Network.Commands;

public struct SpeakRequest
{
    public List<string> TargetFriendCodes { get; set; }
    public string Message { get; set; }
    public ChatMode ChatMode { get; set; }
    public string? Extra { get; set; }

    public SpeakRequest(List<string> targetFriendCodes, string message, ChatMode chatMode, string? extra = null)
    {
        TargetFriendCodes = targetFriendCodes;
        Message = message;
        ChatMode = chatMode;
        Extra = extra;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SpeakRequest");
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("Message", Message);
        sb.AddVariable("Channel", ChatMode);
        sb.AddVariable("Extra", Extra);
        return sb.ToString();
    }
}

public struct SpeakResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public SpeakResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SpeakResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

public struct SpeakCommand
{
    public string SenderFriendCode { get; set; }
    public string Message { get; set; }
    public ChatMode ChatMode { get; set; }
    public string? Extra { get; set; }

    public SpeakCommand(string senderFriendCode, string message, ChatMode chatMode, string? extra)
    {
        SenderFriendCode = senderFriendCode;
        Message = message;
        ChatMode = chatMode;
        Extra = extra;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("SpeakCommand");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("Message", Message);
        sb.AddVariable("Channel", ChatMode);
        sb.AddVariable("Extra", Extra);
        return sb.ToString();
    }
}
