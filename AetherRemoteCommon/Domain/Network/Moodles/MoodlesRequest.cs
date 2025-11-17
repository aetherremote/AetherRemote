using AetherRemoteCommon.Dependencies.Moodles.Domain;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Moodles;

[MessagePackObject(keyAsPropertyName: true)]
public record MoodlesRequest : ActionRequest
{
    public MoodleInfo Info { get; set; } = new();

    public MoodlesRequest()
    {
    }

    public MoodlesRequest(List<string> targets, MoodleInfo info)
    {
        TargetFriendCodes =  targets;
        Info = info;
    }
}