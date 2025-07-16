using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Twinning;

[MessagePackObject(keyAsPropertyName: true)]
public record TwinningForwardedRequest : ForwardedActionRequest
{
    public string CharacterName { get; set; } = string.Empty;
    public CharacterAttributes SwapAttributes { get; set; }
    
    /// <summary>
    ///     Set this code to include a lock on the transform request
    /// </summary>
    public string? LockCode { get; set; }
    
    public TwinningForwardedRequest()
    {
    }

    public TwinningForwardedRequest(string sender, string characterName, CharacterAttributes swapAttributes, string? lockCode)
    {
        SenderFriendCode = sender;
        CharacterName = characterName;
        SwapAttributes = swapAttributes;
        LockCode = lockCode;
    }
}