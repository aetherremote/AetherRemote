using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(true)]
public record ActionRequest
{
    public List<string> TargetFriendCodes { get; set; } = [];
}