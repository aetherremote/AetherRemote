using AetherRemoteCommon.Dependencies.Moodles.Domain;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Moodles;

[MessagePackObject(keyAsPropertyName: true)]
public record MoodlesForwardedRequest : ForwardedActionRequest
{
    public MoodleInfo Info { get; set; } = new();

    public MoodlesForwardedRequest()
    {
    }

    public MoodlesForwardedRequest(string sender, MoodleInfo info)
    {
        SenderFriendCode = sender;
        Info = info;
    }
}