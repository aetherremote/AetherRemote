using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Begin;

[MessagePackObject]
public record PossessionBeginCommand(
    string SenderFriendCode
) : ActionCommand(SenderFriendCode);