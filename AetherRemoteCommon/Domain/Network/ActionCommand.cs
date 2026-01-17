using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject]
public record ActionCommand(
    [property: Key(0)] string SenderFriendCode
);