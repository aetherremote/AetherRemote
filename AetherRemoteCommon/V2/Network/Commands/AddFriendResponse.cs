using AetherRemoteCommon.Domain.Enums;
using MessagePack;
using AddFriendEc = AetherRemoteCommon.V2.Domain.Enums.AddFriendEc;

namespace AetherRemoteCommon.V2.Network.Commands;

[MessagePackObject]
public record AddFriendResponse(
    [property: Key(0)] AddFriendEc Result,
    [property: Key(1)] FriendOnlineStatus Status
);
