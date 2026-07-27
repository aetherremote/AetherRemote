using AetherRemoteCommon.Network.Enums.ErrorCodes;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record RemoveFriendResponse(
    [property: Key(0)] RemoveFriendEc Result
);
