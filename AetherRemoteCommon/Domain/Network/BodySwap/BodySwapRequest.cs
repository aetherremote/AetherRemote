using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.BodySwap;

[MessagePackObject(true)]
public record BodySwapRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] string? SenderCharacterName,
    [property: Key(2)] string? SenderCharacterWorld,
    [property: Key(3)] CharacterAttributes SwapAttributes,
    [property: Key(4)] string? LockCode
) : ActionRequest(TargetFriendCodes);