using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Speak;

[MessagePackObject]
public record SpeakCommand(
    string SenderFriendCode,
    [property: Key(1)] string Message,
    [property: Key(2)] ChatChannel ChatChannel,
    [property: Key(3)] string? Extra
) : ActionCommand(SenderFriendCode);