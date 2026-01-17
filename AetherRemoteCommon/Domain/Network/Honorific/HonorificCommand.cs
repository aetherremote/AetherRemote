using AetherRemoteCommon.Dependencies.Honorific.Domain;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Honorific;

[MessagePackObject]
public record HonorificCommand(
    string SenderFriendCode,
    [property: Key(1)] HonorificInfo Honorific
) : ActionCommand(SenderFriendCode);