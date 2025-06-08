using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.Customize;

[MessagePackObject(true)]
public record CustomizeRequest : ActionRequest
{
    public string Data { get; set; } = string.Empty;

    public CustomizeRequest()
    {
    }

    public CustomizeRequest(List<string> targets, string data)
    {
        TargetFriendCodes = targets;
        Data = data;
    }
}