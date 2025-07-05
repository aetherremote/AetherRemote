using MessagePack;

namespace AetherRemoteCommon.Domain.Network.UpdateFriend;

[MessagePackObject(true)]
public record UpdateFriendResponse
{
    public UpdateFriendEc Result { get; set; }

    public UpdateFriendResponse()
    {
    }

    public UpdateFriendResponse(UpdateFriendEc result)
    {
        Result = result;
    }
}