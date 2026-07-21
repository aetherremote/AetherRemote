using AetherRemoteCommon.Domain.Enums;
using MessagePack;
using AddFriendEc = AetherRemoteCommon.Network.Enums.ErrorCodes.AddFriendEc;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record AddFriendResponse(
    [property: Key(0)] AddFriendEc Result,
    [property: Key(1)] FriendOnlineStatus Status
);
