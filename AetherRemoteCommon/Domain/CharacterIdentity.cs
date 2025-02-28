using MessagePack;

namespace AetherRemoteCommon.Domain;

/// <summary>
///     Represents a character's identity
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public record CharacterIdentity
{
    /// <summary>
    ///     The object name that represents a character in game
    /// </summary>
    public string GameObjectName { get; set; } = string.Empty;
    
    /// <summary>
    ///     The name of a character, used to keep track of multiple body swaps
    /// </summary>
    public string CharacterName { get; set; } = string.Empty;
}