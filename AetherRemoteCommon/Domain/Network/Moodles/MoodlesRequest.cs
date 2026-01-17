using AetherRemoteCommon.Dependencies.Moodles.Domain;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Moodles;

[MessagePackObject]
public record MoodlesRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] MoodleInfo Info
) : ActionRequest(TargetFriendCodes);