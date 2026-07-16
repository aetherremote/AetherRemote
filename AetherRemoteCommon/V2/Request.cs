using MessagePack;

namespace AetherRemoteCommon.V2;

[MessagePackObject]
public record Request<TPayload>(
    [property: Key(0)] List<string> TargetFriendCodes,
    [property: Key(1)] TPayload Payload
);