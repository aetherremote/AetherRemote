using MessagePack;

namespace AetherRemoteCommon.Network.Domain;

[MessagePackObject]
public record Message<TPayload>(
    [property: Key(0)] string SenderFriendCode,
    [property: Key(1)] TPayload Payload
);