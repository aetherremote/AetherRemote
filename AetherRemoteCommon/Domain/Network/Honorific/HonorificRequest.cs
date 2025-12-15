using AetherRemoteCommon.Dependencies.Honorific.Domain;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Honorific;

[MessagePackObject(keyAsPropertyName: true)]
public record HonorificRequest : ActionRequest
{
    public HonorificInfo Honorific { get; set; } = new();
    
    public HonorificRequest()
    {
    }

    public HonorificRequest(List<string> targetFriendCodes, HonorificInfo honorific)
    {
        TargetFriendCodes = targetFriendCodes;
        Honorific = honorific;
    }
}