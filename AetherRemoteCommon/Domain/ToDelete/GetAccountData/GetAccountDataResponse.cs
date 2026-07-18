using MessagePack;

namespace AetherRemoteCommon.Domain.Network.GetAccountData;

[MessagePackObject]
public record GetAccountDataResponse(
    [property: Key(0)] GetAccountDataEc Result,
    [property: Key(1)] string AccountFriendCode,
    [property: Key(2)] ResolvedPermissions AccountGlobalPermissions,
    [property: Key(3)] List<FriendDto> AccountFriends
);