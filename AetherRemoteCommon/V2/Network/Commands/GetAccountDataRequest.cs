using MessagePack;

namespace AetherRemoteCommon.V2.Network.Commands;

[MessagePackObject]
public record GetAccountDataRequest(
    [property: Key(0)] string SenderCharacterName,
    [property: Key(1)] string SenderCharacterWorld
);
