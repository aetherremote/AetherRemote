using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.BodySwap;

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