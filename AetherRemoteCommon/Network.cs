namespace AetherRemoteCommon;

/// <summary>
/// Provides static names for all api methods
/// </summary>
public static class Network
{
    public static class Commands
    {
        public const string Speak = "SpeakCommand";
        public const string Emote = "EmoteCommand";
        public const string Transform = "TransformCommand";
        public const string BodySwap = "BodySwapCommand";
        public const string UpdateOnlineStatus = "UpdateOnlineStatusCommand";
        public const string Revert = "RevertCommand";
    }

    public static class User
    {
        public const string CreateOrUpdate = "CreateOrUpdateUser";
        public const string Delete = "DeleteUser";
        public const string Get = "GetUser";
    }

    public static class Permissions
    {
        public const string CreateOrUpdate = "CreateOrUpdatePermissions";
        public const string Delete = "DeletePermissions";
        public const string Get = "GetPermissions";
    }

    public const string LoginDetails = "LoginDetails";
    public const string BodySwapQuery = "BodySwapQuery";
}
