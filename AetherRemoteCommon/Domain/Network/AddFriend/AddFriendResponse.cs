using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.AddFriend;

[MessagePackObject(keyAsPropertyName: true)]
public record AddFriendResponse
{
    public AddFriendEc Result { get; set; }
    public FriendOnlineStatus Status { get; set; }

    public AddFriendResponse()
    {
    }

    public AddFriendResponse(AddFriendEc code, FriendOnlineStatus status)
    {
        Result = code;
        Status = status;
    }
}