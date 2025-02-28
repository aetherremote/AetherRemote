using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record BaseRequest
{
    public List<string> TargetFriendCodes { get; set; } = [];
}