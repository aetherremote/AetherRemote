using AetherRemoteCommon.Domain.Honorific;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Honorific;

[MessagePackObject]
public record HonorificCommand(
    string SenderFriendCode,
    [property: Key(1)] HonorificDto Honorific
) : ActionCommand(SenderFriendCode);