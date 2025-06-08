using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.AddFriend;

[MessagePackObject(keyAsPropertyName: true)]
public record AddFriendResponse
{
    public AddFriendEc Result { get; set; }
    public bool Online { get; set; }

    public AddFriendResponse()
    {
    }

    public AddFriendResponse(AddFriendEc code, bool online)
    {
        Result = code;
        Online = online;
    }
}