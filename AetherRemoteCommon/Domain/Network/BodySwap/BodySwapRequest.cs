using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.BodySwap;

[MessagePackObject(true)]
public record BodySwapRequest : ActionRequest
{
    public string? SenderCharacterName { get; set; }
    
    public CharacterAttributes SwapAttributes { get; set; }

    public BodySwapRequest()
    {
    }

    public BodySwapRequest(List<string> targets, CharacterAttributes swapAttributes, string? senderCharacterName = null)
    {
        TargetFriendCodes = targets;
        SwapAttributes = swapAttributes;
        SenderCharacterName = senderCharacterName;
    }
}