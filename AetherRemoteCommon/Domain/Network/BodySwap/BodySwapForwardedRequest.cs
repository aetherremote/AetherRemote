using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.BodySwap;

[MessagePackObject(keyAsPropertyName: true)]
public record BodySwapForwardedRequest : ForwardedActionRequest
{
    public string CharacterName { get; set; } =  string.Empty;
    
    public CharacterAttributes SwapAttributes { get; set; }
    
    /// <summary>
    ///     Set this code to include a lock on the transform request
    /// </summary>
    public uint? LockCode { get; set; }

    public BodySwapForwardedRequest()
    {
    }
    
    public BodySwapForwardedRequest(string sender, string characterName, CharacterAttributes swapAttributes, uint? lockCode)
    {
        SenderFriendCode = sender;
        CharacterName = characterName;
        SwapAttributes = swapAttributes;
        LockCode = lockCode;
    }
}