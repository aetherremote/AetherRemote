using AetherRemoteCommon.Domain.Enums;
using MessagePack;

namespace AetherRemoteCommon.Domain.Network.BodySwap;

[MessagePackObject(true)]
public record BodySwapRequest : ActionRequest
{
    public string? SenderCharacterName { get; set; }
    
    public CharacterAttributes SwapAttributes { get; set; }
    
    /// <summary>
    ///     Set this code to include a lock on the transform request
    /// </summary>
    public uint? LockCode { get; set; }

    public BodySwapRequest()
    {
    }

    public BodySwapRequest(List<string> targets, CharacterAttributes swapAttributes, string? senderCharacterName, uint? lockCode)
    {
        TargetFriendCodes = targets;
        SwapAttributes = swapAttributes;
        SenderCharacterName = senderCharacterName;
        LockCode = lockCode;
    }
}