using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.RemoveFriend;

[MessagePackObject(keyAsPropertyName: true)]
public record RemoveFriendResponse
{
    public RemoveFriendEc Result { get; set; }

    public RemoveFriendResponse()
    {
    }

    public RemoveFriendResponse(RemoveFriendEc result)
    {
        Result = result;
    }
}