using MessagePack;

namespace AetherRemoteCommon.Domain.Network.GetAccountData;

[MessagePackObject]
public record GetAccountDataRequest(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld
);