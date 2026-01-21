using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.End;

[MessagePackObject]
public record PossessionEndCommand(
    string SenderFriendCode
) : ActionCommand(SenderFriendCode);