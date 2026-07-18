using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Speak;

[MessagePackObject]
public record SpeakRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] string Message,
    [property: Key(2)] ChatChannel ChatChannel,
    [property: Key(3)] string? Extra
) : ActionRequest(TargetFriendCodes);