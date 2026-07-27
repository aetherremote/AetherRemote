using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record BodySwapResponse(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld
);
