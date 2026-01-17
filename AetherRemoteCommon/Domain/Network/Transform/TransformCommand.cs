using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Transform;

[MessagePackObject]
public record TransformCommand(
    string SenderFriendCode,
    [property: Key(1)] string GlamourerData,
    [property: Key(2)] GlamourerApplyFlags GlamourerApplyType,
    [property: Key(3)] string? LockCode
) : ActionCommand(SenderFriendCode);