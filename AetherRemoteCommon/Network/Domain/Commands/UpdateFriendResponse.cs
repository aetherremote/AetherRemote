using AetherRemoteCommon.Network.Enums.ErrorCodes;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record UpdateFriendResponse(
    [property: Key(0)] UpdateFriendEc Result
);
