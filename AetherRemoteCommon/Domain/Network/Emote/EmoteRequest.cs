using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Emote;

[MessagePackObject(true)]
public record EmoteRequest : ActionRequest
{
    public string Emote { get; set; } = string.Empty;
    public bool DisplayLogMessage { get; set; }

    public EmoteRequest()
    {
    }

    public EmoteRequest(List<string> targets, string emote, bool displayLogMessage)
    {
        TargetFriendCodes = targets;
        Emote = emote;
        DisplayLogMessage = displayLogMessage;
    }
}