using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Moodles;

[MessagePackObject(keyAsPropertyName: true)]
public record MoodlesRequest : ActionRequest
{
    public string Moodle { get; set; } = string.Empty;

    public MoodlesRequest()
    {
    }

    public MoodlesRequest(List<string> targets, string moodle)
    {
        TargetFriendCodes =  targets;
        Moodle = moodle;
    }
}