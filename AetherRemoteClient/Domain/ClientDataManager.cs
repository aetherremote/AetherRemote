using AetherRemoteCommon.Domain.Permissions.V2;

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
        FriendsList.CreateFriend("Friend2", true);
        FriendsList.CreateFriend("Friend3", true);
        FriendsList.CreateFriend("Friend4", false);
        FriendsList.CreateFriend("Friend5", false);

        const PrimaryPermissionsV2 primaryPermissions = PrimaryPermissionsV2.Customization |
                                                        PrimaryPermissionsV2.BodySwap | 
                                                        PrimaryPermissionsV2.Mods;
        
        FriendsList.UpdateLocalPermissions("Friend2", new UserPermissionsV2
        {
            Primary = primaryPermissions
        });
    }
}
