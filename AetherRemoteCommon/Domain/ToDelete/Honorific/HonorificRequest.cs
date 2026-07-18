using AetherRemoteCommon.Domain.Honorific;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Honorific;

[MessagePackObject]
public record HonorificRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] HonorificDto Honorific
) : ActionRequest(TargetFriendCodes);