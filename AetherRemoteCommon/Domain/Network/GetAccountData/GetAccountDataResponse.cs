using MessagePack;

namespace AetherRemoteCommon.Domain.Network.GetAccountData;

[MessagePackObject]
public record GetAccountDataResponse(
    [property: Key(0)] GetAccountDataEc Result,
    [property: Key(1)] string FriendCode,
    [property: Key(2)] List<FriendRelationship> Relationships
);