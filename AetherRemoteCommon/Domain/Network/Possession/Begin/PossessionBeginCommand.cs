using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Begin;

[MessagePackObject]
public record PossessionBeginCommand(
    string SenderFriendCode,
    [property: Key(1)] uint MoveMode
) : ActionCommand(SenderFriendCode);