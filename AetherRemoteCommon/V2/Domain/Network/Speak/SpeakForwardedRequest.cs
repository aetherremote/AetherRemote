using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.Speak;

[MessagePackObject(keyAsPropertyName: true)]
public record SpeakForwardedRequest : ForwardedActionRequest
{
    public string Message { get; set; } = string.Empty;
    public ChatChannel ChatChannel { get; set; }
    public string? Extra  { get; set; }
    
    public SpeakForwardedRequest()
    {
    }

    public SpeakForwardedRequest(string sender, string message, ChatChannel chatChannel, string? extra)
    {
        SenderFriendCode = sender;
        Message = message;
        ChatChannel = chatChannel;
        Extra = extra;
    }
}