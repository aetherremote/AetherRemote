using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Twinning;

[MessagePackObject]
public record TwinningCommand(
    string SenderFriendCode,
    [property: Key(1)] string CharacterName,
    [property: Key(2)] string CharacterWorld,
    [property: Key(3)] CharacterAttributes SwapAttributes,
    [property: Key(4)] string? LockCode
) : ActionCommand(SenderFriendCode);