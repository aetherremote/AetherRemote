using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.GetAccountData;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Manages connection and disconnection events from the server
/// </summary>
public class ConnectionManager : IDisposable
{
    private readonly AccountService _accountService;
    private readonly FriendsListService _friendsListService;
    private readonly NetworkService _networkService;
    private readonly ViewService _viewService;
    
    /// <summary>
    ///     <inheritdoc cref="ConnectionManager"/>
    /// </summary>
    public ConnectionManager(AccountService accountService, FriendsListService friendsListService, NetworkService networkService, ViewService viewService)
    {
        _accountService = accountService;
        _friendsListService = friendsListService;
        _networkService = networkService;
        _viewService = viewService;

        _networkService.Connected += OnConnected;
        _networkService.Disconnected += OnDisconnected;
    }
    
    private async Task OnConnected()
    {
        if (Plugin.CharacterConfiguration is not { } character)
            return;
        
        // Get account data from the server
        var request = new GetAccountDataRequest(character.Name, character.World);
        var response = await _networkService.InvokeAsync<GetAccountDataResponse>(HubMethod.GetAccountData, request).ConfigureAwait(false);

        // If there wasn't a success, don't stay connected; the plugin is not usable in this state
        if (response.Result is not GetAccountDataEc.Success)
        {
            Plugin.Log.Fatal($"[ConnectionManager] Failed to get account data {response.Result}");
            await _networkService.StopAsync().ConfigureAwait(false);
            return;
        }
        
        // Set our account information
        _accountService.SetFriendCode(response.AccountFriendCode);
        _accountService.SetGlobalPermissions(response.AccountGlobalPermissions);
        
        // Clear the friend list in preparation for adding friends returned from the server
        _friendsListService.Clear();

        // Iterate over all the relationships to transform them into domain models
        foreach (var friend in response.AccountFriends)
        {
            // Try to extract the note
            Plugin.Configuration.Notes.TryGetValue(friend.TargetFriendCode, out var note);
            
            // Add the new friend with all the data required
            _friendsListService.Add(new Friend(friend.TargetFriendCode, friend.Status, note, friend.PermissionsGrantedTo, friend.PermissionsGrantedBy));
        }

        // Set the view to the 'home screen'
        _viewService.CurrentView = View.Status;
    }

    private Task OnDisconnected()
    {
        // Clear the friend list
        _friendsListService.Clear();
        
        // Reset the view if required
        _viewService.ResetView();
        
        // Return
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _networkService.Connected -= OnConnected;
        _networkService.Disconnected -= OnDisconnected;
        GC.SuppressFinalize(this);
    }
}