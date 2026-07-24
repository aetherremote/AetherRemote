using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Commands;

[MessagePackObject]
public record InitializeSessionRequest(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld
);
