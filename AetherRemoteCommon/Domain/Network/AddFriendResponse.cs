using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

[MessagePackObject(keyAsPropertyName: true)]
public record AddFriendResponse : BaseResponse
{
    public bool Online { get; set; }
}