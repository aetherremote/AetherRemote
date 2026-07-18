using MessagePack;

namespace AetherRemoteCommon.Domain.Network.UpdateFriend;

[MessagePackObject(true)]
public record UpdateFriendResponse(
    [property: Key(0)] UpdateFriendEc Result
);