using System;
using System.Collections.Generic;
using AetherRemoteClient.Handlers.Network;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.External;
using AetherRemoteCommon.Domain.Network;
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

    /// <summary>
    ///     <inheritdoc cref="NetworkHandler"/>
    /// </summary>
    public NetworkHandler(
        EmoteService emoteService,
        FriendsListService friendsListService,
        GlamourerService glamourerService,
        IdentityService identityService,
        MoodlesService moodlesService,
        OverrideService overrideService,
        PenumbraService penumbraService,
        LogService logService,
        NetworkService networkService,
        ActionQueueManager actionQueueManager,
        ModManager modManager)
    {
        var bodySwapHandler = new BodySwapHandler(friendsListService, identityService, overrideService, logService, modManager);
        var bodySwapQueryHandler = new BodySwapQueryHandler(friendsListService, identityService, overrideService, logService);
        var emoteHandler = new EmoteHandler(emoteService, friendsListService, logService, overrideService);
        var moodlesHandler = new MoodlesHandler(friendsListService, moodlesService, overrideService, penumbraService, logService);
        var speakHandler = new SpeakHandler(friendsListService, logService, overrideService, actionQueueManager);
        var syncOnlineStatusHandler = new SyncOnlineStatusHandler(friendsListService);
        var syncPermissionsHandler = new SyncPermissionsHandler(friendsListService);
        var transformHandler = new TransformHandler(friendsListService, glamourerService, overrideService, logService);
        var twinningHandler = new TwinningHandler(friendsListService, identityService, overrideService, logService, modManager);

        _handlers.Add(networkService.Connection.On<BodySwapQueryRequest, BodySwapQueryResponse>(HubMethod.BodySwapQuery, bodySwapQueryHandler.Handle));
        _handlers.Add(networkService.Connection.On<BodySwapAction>(HubMethod.BodySwap, bodySwapHandler.Handle));
        _handlers.Add(networkService.Connection.On<EmoteAction>(HubMethod.Emote, emoteHandler.Handle));
        _handlers.Add(networkService.Connection.On<MoodlesAction>(HubMethod.Moodles, moodlesHandler.Handle));
        _handlers.Add(networkService.Connection.On<SpeakAction>(HubMethod.Speak, speakHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncOnlineStatusAction>(HubMethod.SyncOnlineStatus, syncOnlineStatusHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncPermissionsAction>(HubMethod.SyncPermissions, syncPermissionsHandler.Handle));
        _handlers.Add(networkService.Connection.On<TransformAction>(HubMethod.Transform, transformHandler.Handle));
        _handlers.Add(networkService.Connection.On<TwinningAction>(HubMethod.Twinning, twinningHandler.Handle));
    }

    public void Dispose()
    {
        foreach (var subscription in _handlers)
            subscription.Dispose();

        GC.SuppressFinalize(this);
    }
}