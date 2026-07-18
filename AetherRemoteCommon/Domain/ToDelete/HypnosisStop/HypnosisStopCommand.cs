using MessagePack;

namespace AetherRemoteCommon.Domain.Network.HypnosisStop;

[MessagePackObject]
public record HypnosisStopCommand(
    string SenderFriendCode
) : ActionCommand(SenderFriendCode);