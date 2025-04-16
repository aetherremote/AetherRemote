using System;
using System.Collections.Generic;
using AetherRemoteClient.Handlers.Network;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
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
        CustomizePlusIpc customizePlusIpc,
        EmoteService emoteService,
        FriendsListService friendsListService,
        IdentityService identityService,
        OverrideService overrideService,
        LogService logService,
        NetworkService networkService,
        SpiralService spiralService,
        GlamourerIpc glamourerIpc,
        MoodlesIpc moodlesIpc,
        PenumbraIpc penumbraIpc,
        ActionQueueManager actionQueueManager,
        ModManager modManager)
    {
        var bodySwapHandler = new BodySwapHandler(friendsListService, identityService, overrideService, logService, modManager);
        var bodySwapQueryHandler = new BodySwapQueryHandler(friendsListService, identityService, overrideService, logService);
        var emoteHandler = new EmoteHandler(emoteService, friendsListService, logService, overrideService);
        var hypnosisHandler = new HypnosisHandler(friendsListService, overrideService, logService, spiralService);
        var moodlesHandler = new MoodlesHandler(friendsListService, overrideService, logService, moodlesIpc, penumbraIpc);
        var speakHandler = new SpeakHandler(friendsListService, logService, overrideService, actionQueueManager);
        var syncOnlineStatusHandler = new SyncOnlineStatusHandler(friendsListService);
        var syncPermissionsHandler = new SyncPermissionsHandler(friendsListService);
        var transformHandler = new TransformHandler(friendsListService, overrideService, logService, glamourerIpc);
        var twinningHandler = new TwinningHandler(friendsListService, identityService, overrideService, logService, modManager);
        var customizePlusHandler = new CustomizePlusHandler(friendsListService, overrideService, logService, customizePlusIpc);

        _handlers.Add(networkService.Connection.On<BodySwapQueryRequest, BodySwapQueryResponse>(HubMethod.BodySwapQuery, bodySwapQueryHandler.Handle));
        _handlers.Add(networkService.Connection.On<BodySwapAction>(HubMethod.BodySwap, bodySwapHandler.Handle));
        _handlers.Add(networkService.Connection.On<EmoteAction>(HubMethod.Emote, emoteHandler.Handle));
        _handlers.Add(networkService.Connection.On<MoodlesAction>(HubMethod.Moodles, moodlesHandler.Handle));
        _handlers.Add(networkService.Connection.On<SpeakAction>(HubMethod.Speak, speakHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncOnlineStatusAction>(HubMethod.SyncOnlineStatus, syncOnlineStatusHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncPermissionsAction>(HubMethod.SyncPermissions, syncPermissionsHandler.Handle));
        _handlers.Add(networkService.Connection.On<TransformAction>(HubMethod.Transform, transformHandler.Handle));
        _handlers.Add(networkService.Connection.On<TwinningAction>(HubMethod.Twinning, twinningHandler.Handle));
        _handlers.Add(networkService.Connection.On<CustomizePlusAction>(HubMethod.CustomizePlus, customizePlusHandler.Handle));
        _handlers.Add(networkService.Connection.On<HypnosisAction>(HubMethod.Hypnosis, hypnosisHandler.Handle));
    }

    public void Dispose()
    {
        foreach (var subscription in _handlers)
            subscription.Dispose();

        GC.SuppressFinalize(this);
    }
}