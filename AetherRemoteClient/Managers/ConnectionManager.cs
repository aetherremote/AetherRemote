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
    private readonly FriendsListService _friendsListService;
    private readonly IdentityService _identityService;
    private readonly NetworkService _networkService;
    private readonly ViewService _viewService;
    
    /// <summary>
    ///     <inheritdoc cref="ConnectionManager"/>
    /// </summary>
    public ConnectionManager(FriendsListService friendsListService, IdentityService identityService, NetworkService networkService, ViewService viewService)
    {
        _friendsListService = friendsListService;
        _identityService = identityService;
        _networkService = networkService;
        _viewService = viewService;

        _networkService.Connected += OnConnected;
        _networkService.Disconnected += OnDisconnected;
    }
    
    private async Task OnConnected()
    {
        // Get the local player
        if (await Plugin.RunOnFramework(() => Plugin.ObjectTable.LocalPlayer).ConfigureAwait(false) is not { } player)
            return;
        
        // Get account data from the server
        // TODO: Expand this to include the world as well to prevent same-name alts from disrupting workflow
        var request = new GetAccountDataRequest(player.Name.ToString());
        var response = await _networkService.InvokeAsync<GetAccountDataResponse>(HubMethod.GetAccountData, request).ConfigureAwait(false);

        // If there wasn't a success, don't stay connected; the plugin is not usable in this state
        if (response.Result is not GetAccountDataEc.Success)
        {
            Plugin.Log.Fatal($"[ConnectionManager] Failed to get account data {response.Result}");
            await _networkService.StopAsync().ConfigureAwait(false);
            return;
        }

        // Set the friend code
        _identityService.SetFriendCode(response.FriendCode);
        
        // Clear the friend list in preparation for adding friends returned from the server
        _friendsListService.Clear();

        // Iterate over all the relationships to transform them into domain models
        foreach (var relationship in response.Relationships)
        {
            Plugin.Log.Info($"{relationship.TargetFriendCode} is {relationship.Status}");
            
            // Try to extract the note
            Plugin.Configuration.Notes.TryGetValue(relationship.TargetFriendCode, out var note);
            
            // Add the new friend with all the data required
            _friendsListService.Add(new Friend(relationship.TargetFriendCode, relationship.Status, note, relationship.PermissionsGrantedTo, relationship.PermissionsGrantedBy));
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