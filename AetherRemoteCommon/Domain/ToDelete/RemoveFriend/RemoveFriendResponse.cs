using MessagePack;

namespace AetherRemoteCommon.Domain.Network.RemoveFriend;

[MessagePackObject]
public record RemoveFriendResponse(
    [property: Key(0)] RemoveFriendEc Result
);