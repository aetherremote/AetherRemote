using MessagePack;

namespace AetherRemoteCommon.V2.Network.Relays;

[MessagePackObject]
public record BodySwapResponse(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld
);
