using AetherRemoteCommon.Dependencies.Honorific.Domain;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Honorific;

[MessagePackObject]
public record HonorificRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] HonorificInfo Honorific
) : ActionRequest(TargetFriendCodes);