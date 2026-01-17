using MessagePack;

namespace AetherRemoteCommon.Domain.Network.HypnosisStop;

[MessagePackObject]
public record HypnosisStopRequest(
    List<string> TargetFriendCodes
) : ActionRequest(TargetFriendCodes);