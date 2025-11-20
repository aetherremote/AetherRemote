using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Handlers.Network;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.GetAccountData;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using AetherRemoteCommon.Domain.Network.Moodles;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteCommon.Domain.Network.SyncPermissions;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Domain.Network.Twinning;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers;

/// <summary>
///     Handles incoming requests from the server
/// </summary>
public class NetworkHandler : IDisposable
{
    /// <summary>
    ///     List of subscriptions to incoming server events
    /// </summary>
    private readonly List<IDisposable> _handlers = [];

    // Injected
    private readonly FriendsListService _friendsListService;
    private readonly IdentityService  _identityService;
    private readonly NetworkService _networkService;
    private readonly ViewService _viewService;
    
    /// <summary>
    ///     <inheritdoc cref="NetworkHandler"/>
    /// </summary>
    public NetworkHandler(
        FriendsListService friendsListService,
        IdentityService identityService,
        NetworkService networkService,
        ViewService viewService,
        BodySwapHandler bodySwapHandler,
        EmoteHandler emoteHandler,
        HypnosisHandler hypnosisHandler,
        MoodlesHandler moodlesHandler,
        SpeakHandler speakHandler,
        SyncOnlineStatusHandler syncOnlineStatusHandler,
        SyncPermissionsHandler syncPermissionsHandler,
        TransformHandler transformHandler,
        TwinningHandler twinningHandler,
        CustomizePlusHandler customizePlusHandler)
    {
        _friendsListService = friendsListService;
        _identityService = identityService;
        _networkService = networkService;
        _viewService = viewService;
        
        _networkService.Connected += OnConnected;
        _networkService.Disconnected += OnDisconnected;
        
        // Responses Needed
        _handlers.Add(networkService.Connection.On<BodySwapForwardedRequest, ActionResult<Unit>>(HubMethod.BodySwap, bodySwapHandler.Handle));
        _handlers.Add(networkService.Connection.On<CustomizeForwardedRequest, ActionResult<Unit>>(HubMethod.CustomizePlus, customizePlusHandler.Handle));
        _handlers.Add(networkService.Connection.On<EmoteForwardedRequest, ActionResult<Unit>>(HubMethod.Emote, emoteHandler.Handle));
        _handlers.Add(networkService.Connection.On<HypnosisForwardedRequest, ActionResult<Unit>>(HubMethod.Hypnosis, hypnosisHandler.Handle));
        _handlers.Add(networkService.Connection.On<MoodlesForwardedRequest, ActionResult<Unit>>(HubMethod.Moodles, moodlesHandler.Handle)); // TODO: Readd
        _handlers.Add(networkService.Connection.On<SpeakForwardedRequest, ActionResult<Unit>>(HubMethod.Speak, speakHandler.Handle));
        _handlers.Add(networkService.Connection.On<TransformForwardedRequest, ActionResult<Unit>>(HubMethod.Transform, transformHandler.Handle));
        _handlers.Add(networkService.Connection.On<TwinningForwardedRequest, ActionResult<Unit>>(HubMethod.Twinning, twinningHandler.Handle));
        
        // No Responses Needed
        _handlers.Add(networkService.Connection.On<SyncOnlineStatusForwardedRequest>(HubMethod.SyncOnlineStatus, syncOnlineStatusHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncPermissionsForwardedRequest>(HubMethod.SyncPermissions, syncPermissionsHandler.Handle));
    }
    
    private async Task OnConnected()
    {
        // Get the local player
        if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer) is not { } player)
            return;
        
        // TODO: Expand to include world in the future to help with same-name issues
        // Pass the character name to the login request
        var input = new GetAccountDataRequest(player.Name.ToString());
        
        // Get account data from the server
        var response = await _networkService
            .InvokeAsync<GetAccountDataResponse>(HubMethod.GetAccountData, input)
            .ConfigureAwait(false);
        
        // If there wasn't a success, don't stay connected; the plugin is not usable in this state
        if (response.Result is not GetAccountDataEc.Success)
        {
            Plugin.Log.Fatal($"[NetworkHandler] Failed to get account data, {response.Result}");
            await _networkService.StopAsync().ConfigureAwait(false);
            return;
        }

        // Set the friend code
        _identityService.FriendCode = response.FriendCode;

        // Clear the friend list in preparation for adding friends returned from the server
        _friendsListService.Clear();
        
        // Iterate over all the permissions we've granted to others
        foreach (var (friendCode, permissionsGrantedToFriend) in response.PermissionsGrantedToOthers)
        {
            // Check if they're online. Server will only include online friends in this permission-granted-by-others dictionary
            var online = response.PermissionsGrantedByOthers.TryGetValue(friendCode, out var permissionsGrantedByOther);
            
            // Try to extract the note
            Plugin.Configuration.Notes.TryGetValue(friendCode, out var note);

            // Create a new friends object with everything we've gathered
            var friend = new Friend(friendCode, online, note, permissionsGrantedToFriend, permissionsGrantedByOther);
            
            // Add to our friend list
            _friendsListService.Add(friend);
        }

        // Set the view to the status page
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
        
        foreach (var subscription in _handlers)
            subscription.Dispose();

        GC.SuppressFinalize(this);
    }
}