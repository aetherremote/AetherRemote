using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.BodySwap;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapForwardedRequest : ForwardedActionRequest
{
    public string CharacterName { get; set; } =  string.Empty;
    
    public CharacterAttributes SwapAttributes { get; set; }

    public BodySwapForwardedRequest()
    {
    }
    
    public BodySwapForwardedRequest(string sender, string characterName, CharacterAttributes swapAttributes)
    {
        SenderFriendCode = sender;
        CharacterName = characterName;
        SwapAttributes = swapAttributes;
    }
}