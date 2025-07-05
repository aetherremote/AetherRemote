using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Emote;

[MessagePackObject(true)]
public record EmoteForwardedRequest : ForwardedActionRequest
{
    public string Emote { get; set; } = string.Empty;
    public bool DisplayLogMessage { get; set; }

    public EmoteForwardedRequest()
    {
    }

    public EmoteForwardedRequest(string sender, string emote, bool displayLogMessage)
    {
        SenderFriendCode = sender;
        Emote = emote;
        DisplayLogMessage = displayLogMessage;
    }
}