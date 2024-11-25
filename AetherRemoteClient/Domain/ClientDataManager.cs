using System;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Permissions;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Responsible for managing all local client data such as friend code, friend list, etc.
/// </summary>
public class ClientDataManager : IDisposable
{
    // Injected
    private readonly NetworkProvider _networkProvider;

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
    public ClientDataManager(NetworkProvider networkProvider)
    {
        FriendCode = null;
        SafeMode = false;
        FriendsList = new FriendsList();
        TargetManager = new TargetManager();

        _networkProvider = networkProvider;
        _networkProvider.ServerConnected += OnServerConnected;
        _networkProvider.ServerDisconnected += OnServerDisconnected;

        if (Plugin.DeveloperMode is false) return;
        FriendCode = "Dev Mode";
        FriendsList.CreateFriend("Friend1", false);
        FriendsList.CreateFriend("Friend2", true);
        FriendsList.CreateFriend("Friend3", true);
        FriendsList.CreateFriend("Friend4", false);
        FriendsList.CreateFriend("Friend5", false);

        const PrimaryPermissions primaryPermissions = PrimaryPermissions.Customization |
                                                      PrimaryPermissions.BodySwap |
                                                      PrimaryPermissions.Mods;

        FriendsList.UpdateLocalPermissions("Friend2", new UserPermissions
        {
            Primary = primaryPermissions
        });
    }

    private async void OnServerConnected(object? sender, EventArgs e)
    {
        try
        {
            var request = new LoginDetailsRequest();
            var response =
                await _networkProvider.InvokeCommand<LoginDetailsRequest, LoginDetailsResponse>(Network.LoginDetails,
                    request);

            if (response.Success is false)
            {
                Plugin.Log.Warning($"Unable to retrieve login details: {response.Message}");
                return;
            }

            FriendCode = response.FriendCode;
            FriendsList.ConvertServerPermissionsToLocal(response.PermissionsGrantedToOthers,
                response.PermissionsGrantedByOthers);
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"[ClientDataManager] Exception on getting login details: {exception}");
        }
    }

    private void OnServerDisconnected(object? sender, EventArgs e)
    {
        FriendCode = null;
        FriendsList.Clear();
        TargetManager.Clear();
    }

    public void Dispose()
    {
        _networkProvider.ServerConnected -= OnServerConnected;
        _networkProvider.ServerDisconnected -= OnServerDisconnected;
        GC.SuppressFinalize(this);
    }
}