namespace AetherRemoteCommon;

public static class Constraints
{
    /// <summary>
    /// Limits the amount of characters when entering a friend code
    /// </summary>
    public const uint FriendCodeCharLimit = 16;

    /// <summary>
    /// Limits the amount of characters when entering a friend's nickname
    /// </summary>
    public const uint FriendNicknameCharLimit = 24;

    /// <summary>
    /// Limits the amount of characters in a speak command message
    /// </summary>
    public const uint SpeakCommandCharLimit = 400;

    /// <summary>
    /// Limits the amount of characters when entering a secret
    /// </summary>
    public const uint SecretCharLimit = 36;

    /// <summary>
    /// Limits the amount of characters when entering glamourer data
    /// </summary>
    public const uint GlamourerDataCharLimit = 2000;

    /// <summary>
    /// Limits the amount of characters that should be in an emote (In-game max is 17)
    /// </summary>
    public const uint EmoteCharLimit = 20;

    /// <summary>
    /// Limits the amount of characters that should be in an in-game player name
    /// </summary>
    public const uint PlayerNameCharLimit = 25;

    /// <summary>
    /// Limits the amount of characters that should be in a tell target
    /// </summary>
    public const uint TellTargetLimit = PlayerNameCharLimit * 2;

    /// <summary>
    /// Limit the amount of players targetable with in-game operations temporarily
    /// </summary>
    public const uint MaximumTargetsForInGameOperations = 3;

    /// <summary>
    /// How many seconds inbetween each command must a user wait to use in-game commands (Speak, Emote, etc)
    /// </summary>
    public const uint GameCommandCooldownInSeconds = 3;

    /// <summary>
    /// How many seconds inbetween each command must a user wait to use external commands (Glamourer, etc)
    /// </summary>
    public const uint ExternalCommandCooldownInSeconds = 3;
}
