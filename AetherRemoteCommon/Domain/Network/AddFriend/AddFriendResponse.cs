using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.AddFriend;

[MessagePackObject]
public record AddFriendResponse(
    [property: Key(0)] AddFriendEc Result,
    [property: Key(0)] FriendOnlineStatus Status
);