using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Network.Enums.ErrorCodes;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record InitializeSessionResponse(
    [property: Key(0)] GetAccountDataEc Result,
    [property: Key(1)] string AccountFriendCode,
    [property: Key(2)] ResolvedPermissions AccountGlobalPermissions,
    [property: Key(3)] List<FriendDto> AccountFriends
);
