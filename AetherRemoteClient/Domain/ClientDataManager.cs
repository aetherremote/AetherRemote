using AetherRemoteCommon.Domain;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Responsible for managing all local client data such as friend code, friend list, etc.
/// </summary>
public class ClientDataManager
{
    /// <summary>
    /// Local Client's Friend Code
    /// </summary>
    public string? FriendCode;

    /// <summary>
    /// Is the plugin in safe mode?
    /// </summary>
    public bool SafeMode;

    /// <summary>
    /// Local Client's Friends List
    /// </summary>
    public readonly FriendsList FriendsList;

    /// <summary>
    /// Local Client's Targets
    /// </summary>
    public readonly TargetManager TargetManager;

    /// <summary>
    /// <inheritdoc cref="ClientDataManager"/>
    /// </summary>
    public ClientDataManager()
    {
        FriendCode = null;
        SafeMode = false;
        FriendsList = new FriendsList();
        TargetManager = new TargetManager();

        if (Plugin.DeveloperMode == false) return;
        FriendCode = "Dev Mode";
        FriendsList.CreateFriend("Friend1", false);
        FriendsList.CreateFriend("Friend2", true, UserPermissions.Equipment | UserPermissions.Customization | UserPermissions.ModSwap);
        FriendsList.CreateFriend("Friend3", true, UserPermissions.Equipment | UserPermissions.Customization);
        FriendsList.CreateFriend("Friend4", false);
        FriendsList.CreateFriend("Friend5", false);
    }
}
