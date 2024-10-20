namespace AetherRemoteCommon.Domain.Network.Commands;

public struct EmoteRequest
{
    public List<string> TargetFriendCodes { get; set; }
    public string Emote { get; set; }
    public bool DisplayLogMessage { get; set; }

    public EmoteRequest(List<string> targetFriendCodes, string emote, bool displayLogMessage)
    {
        TargetFriendCodes = targetFriendCodes;
        Emote = emote;
        DisplayLogMessage = displayLogMessage;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("EmoteRequest");
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("Emote", Emote);
        sb.AddVariable("DisplayLogMessage", DisplayLogMessage);
        return sb.ToString();
    }
}

public struct EmoteResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public EmoteResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("EmoteResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

public struct EmoteCommand
{
    public string SenderFriendCode { get; set; }
    public string Emote { get; set; }
    public bool DisplayLogMessage { get; set; }

    public EmoteCommand(string senderFriendCode, string emote, bool displayLogMessage)
    {
        SenderFriendCode = senderFriendCode;
        Emote = emote;
        DisplayLogMessage = displayLogMessage;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("EmoteCommand");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("Emote", Emote);
        sb.AddVariable("DisplayLogMessage", DisplayLogMessage);
        return sb.ToString();
    }
}
