using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Twinning;

[MessagePackObject(keyAsPropertyName: true)]
public record TwinningRequest : ActionRequest
{
    public string CharacterName { get; set; } = string.Empty;
   
    public CharacterAttributes SwapAttributes { get; set; }
    
    /// <summary>
    ///     Set this code to include a lock on the transform request
    /// </summary>
    public string? LockCode { get; set; }
    
    public TwinningRequest()
    {
    }

    public TwinningRequest(List<string> targets, string characterName, CharacterAttributes swapAttributes, string? lockCode)
    {
        TargetFriendCodes = targets;
        CharacterName = characterName;
        SwapAttributes = swapAttributes;
        LockCode = lockCode;
    }
}