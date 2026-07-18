using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Commands;

[MessagePackObject]
public record UpdateFriendRequest(
    [property: Key(0)] RawPermissions Permissions
);
