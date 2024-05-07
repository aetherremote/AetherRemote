namespace AetherRemoteCommon;

public static class Constants
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
    public const uint GlamourerDataCharLimit = 1200;

    /// <summary>
    /// Limits the amount of characters that should be in an emote (In-game max is 17)
    /// </summary>
    public const uint EmoteCharLimit = 20;

    /// <summary>
    /// Limits the amount of characters that should be in an in-game player name
    /// </summary>
    public const uint PlayerNameCharLimit = 25;

    // API Methods
    public const string ApiLogin = "Login";
    public const string ApiSync = "Sync";
    public const string ApiUploadFriendList = "UploadFriendList";
    public const string ApiDownloadFriendList = "DownloadFriendList";
    public const string ApiCreateOrUpdateFriend = "CreateOrUpdateFriend";
    public const string ApiDeleteFriend = "DeleteFriend";
    public const string ApiBecome = "Become";
    public const string ApiEmote = "Emote";
    public const string ApiSpeak = "Speak";
    public const string ApiOnlineStatus = "OnlineStatus";
}
