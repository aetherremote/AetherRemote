using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Hypnosis;

[MessagePackObject]
public record HypnosisCommand(
    string SenderFriendCode,
    [property: Key(1)] HypnosisData Data
) : ActionCommand(SenderFriendCode);