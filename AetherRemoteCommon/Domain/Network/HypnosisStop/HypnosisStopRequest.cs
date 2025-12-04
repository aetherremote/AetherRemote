using MessagePack;

namespace AetherRemoteCommon.Domain.Network.HypnosisStop;

[MessagePackObject(keyAsPropertyName: true)]
public record HypnosisStopRequest : ActionRequest
{
    public HypnosisStopRequest()
    {
    }

    public HypnosisStopRequest(List<string> targetFriendCodes)
    {
        TargetFriendCodes = targetFriendCodes;
    }
}