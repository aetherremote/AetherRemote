using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.BodySwap;

[MessagePackObject]
public record BodySwapResponse(
    ActionResponseEc Result,
    Dictionary<string, ActionResultEc> Results,
    [property: Key(2)] string? CharacterName,
    [property: Key(3)] string? CharacterWorld
) : ActionResponse (Result, Results);