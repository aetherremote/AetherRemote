using AetherRemoteCommon.V2.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Commands;

[MessagePackObject]
public record RemoveFriendResponse(
    [property: Key(0)] RemoveFriendEc Result
);
