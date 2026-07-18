using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Transform;

[MessagePackObject]
public record TransformRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] string GlamourerData,
    [property: Key(2)] GlamourerApplyFlags GlamourerApplyType,
    [property: Key(3)] string? LockCode
) : ActionRequest(TargetFriendCodes);