using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Movement;

[MessagePackObject]
public record PossessionMovementCommand(
    string SenderFriendCode,
    [property: Key(1)] float Horizontal,
    [property: Key(2)] float Vertical,
    [property: Key(3)] float Turn,
    [property: Key(4)] byte Backwards
) : ActionCommand(SenderFriendCode);