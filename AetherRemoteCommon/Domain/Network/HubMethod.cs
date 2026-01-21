namespace AetherRemoteCommon.Domain.Network;

/// <summary>
///     The names of the hub methods the server / client can call
/// </summary>
public static class HubMethod
{
    public const string AddFriend = "AddFriend";
    public const string UpdateFriend = "UpdateFriend";
    public const string RemoveFriend = "RemoveFriend";

    public const string Speak = "Speak";
    public const string Emote = "Emote";
    public const string Transform = "Transform";
    public const string BodySwap = "BodySwap";
    public const string Twinning = "Twinning";

    public const string GetAccountData = "GetAccountData";
    
    public const string SyncOnlineStatus = "SyncOnlineStatus";
    public const string SyncPermissions = "SyncPermissions";
    
    public const string Moodles = "Moodles";
    public const string CustomizePlus = "CustomizePlus";
    public const string Hypnosis = "Hypnosis";
    public const string HypnosisStop = "HypnosisStop";
    public const string Honorific = "Honorific";

    public static class Possession
    {
        public const string Begin = "PossessionBegin";
        public const string Camera = "PossessionCamera";
        public const string End = "PossessionEnd";
        public const string Movement = "PossessionMovement";
    }
}