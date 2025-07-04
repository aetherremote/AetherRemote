namespace AetherRemoteCommon;

/// <summary>
///     Various constraints used throughout the plugin
/// </summary>
public static class Constraints
{
    /// <summary>
    ///     Limit the number of players targetable with in-game operations temporarily
    /// </summary>
    public const uint MaximumTargetsForInGameOperations = 3;

    /// <summary>
    ///     How many seconds in between each command must a user wait to use in-game commands (Speak, Emote, etc.)
    /// </summary>
    public const uint GameCommandCooldownInSeconds = 3;

    /// <summary>
    ///     How many seconds in between each command must a user wait to use external commands (Glamourer, etc.)
    /// </summary>
    public const uint ExternalCommandCooldownInSeconds = 3;
}
