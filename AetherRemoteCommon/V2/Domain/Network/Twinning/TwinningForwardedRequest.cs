using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.V2.Domain.Network.Twinning;

[MessagePackObject(keyAsPropertyName: true)]
public record TwinningForwardedRequest : ForwardedActionRequest
{
    public string CharacterName { get; set; } = string.Empty;
    public CharacterAttributes SwapAttributes { get; set; }
    
    public TwinningForwardedRequest()
    {
    }

    public TwinningForwardedRequest(string sender, string characterName, CharacterAttributes swapAttributes)
    {
        SenderFriendCode = sender;
        CharacterName = characterName;
        SwapAttributes = swapAttributes;
    }
}