using AetherRemoteCommon.V2.Domain.Enum;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.Base;

[MessagePackObject(true)]
public record ActionResponse
{
    public ActionResponseEc Result { get; set; }

    public Dictionary<string, ActionResultEc> Results { get; set; } = [];

    public ActionResponse()
    {
    }

    public ActionResponse(ActionResponseEc code)
    {
        Result = code;
    }
    
    public ActionResponse(Dictionary<string, ActionResultEc> results)
    {
        Result = ActionResponseEc.Success;
        Results = results;
    }
}