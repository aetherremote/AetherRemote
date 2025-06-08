using AetherRemoteCommon.V2.Domain.Enum;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.BodySwap;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapResponse : ActionResponse
{
    public string? CharacterName { get; set; }

    public BodySwapResponse()
    {
    }

    public BodySwapResponse(ActionResponseEc code)
    {
        Result = code;
    }
    
    public BodySwapResponse(Dictionary<string, ActionResultEc> results, string? characterName = null)
    {
        Result = ActionResponseEc.Success;
        Results = results;
        CharacterName = characterName;
    }
}