using AetherRemoteCommon.Domain;
using AetherRemoteCommon.V2.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Network.Commands;

[MessagePackObject]
public record GetAccountDataResponse(
    [property: Key(0)] GetAccountDataEc Result,
    [property: Key(1)] string AccountFriendCode,
    [property: Key(2)] ResolvedPermissions AccountGlobalPermissions,
    [property: Key(3)] List<FriendDto> AccountFriends
);
