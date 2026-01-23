using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Begin;

[MessagePackObject]
public record PossessionBeginResponse(
    PossessionResponseEc Response,
    PossessionResultEc Result,
    [property: Key(2)] string CharacterName,
    [property: Key(3)] string CharacterWorld
) : PossessionResponse(Response, Result);