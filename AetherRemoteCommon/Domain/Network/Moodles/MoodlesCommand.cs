using AetherRemoteCommon.Dependencies.Moodles.Domain;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Moodles;

[MessagePackObject]
public record MoodlesCommand(
    string SenderFriendCode,
    [property: Key(1)] MoodleInfo Info
) : ActionCommand(SenderFriendCode);