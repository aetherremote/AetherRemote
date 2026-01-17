using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Hypnosis;

[MessagePackObject]
public record HypnosisRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] HypnosisData Data
) : ActionRequest(TargetFriendCodes);