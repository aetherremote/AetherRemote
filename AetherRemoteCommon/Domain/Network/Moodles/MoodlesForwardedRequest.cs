using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Moodles;

[MessagePackObject(keyAsPropertyName: true)]
public record MoodlesForwardedRequest : ForwardedActionRequest
{
    public string Moodle { get; set; } = string.Empty;

    public MoodlesForwardedRequest()
    {
    }

    public MoodlesForwardedRequest(string sender, string moodle)
    {
        SenderFriendCode = sender;
        Moodle = moodle;
    }
}