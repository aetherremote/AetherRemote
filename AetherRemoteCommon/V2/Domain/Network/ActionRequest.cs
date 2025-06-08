using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network;

[MessagePackObject(true)]
public record ActionRequest
{
    public List<string> TargetFriendCodes { get; set; } = [];
}