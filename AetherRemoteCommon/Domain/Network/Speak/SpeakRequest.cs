using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Speak;

[MessagePackObject(keyAsPropertyName: true)]
public record SpeakRequest : ActionRequest
{
    public string Message { get; set; } = string.Empty;
    public ChatChannel ChatChannel { get; set; }
    public string? Extra  { get; set; }

    public SpeakRequest()
    {
    }

    public SpeakRequest(List<string> targets, string message, ChatChannel chatChannel, string? extra)
    {
        TargetFriendCodes = targets;
        Message = message;
        ChatChannel = chatChannel;
        Extra = extra;
    }
}