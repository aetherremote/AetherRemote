using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject]
public record ActionRequest(
    [property: Key(0)] List<string> TargetFriendCodes
);