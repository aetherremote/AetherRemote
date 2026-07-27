using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record MimicryResponse(
    [property: Key(0)] string CharacterName,
    [property: Key(1)] string CharacterWorld
);