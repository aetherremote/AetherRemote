using System;
using System.Collections.Generic;
using AetherRemoteClient.Handlers.Network;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Domain.Network;
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
        NetworkService networkService,
        BodySwapHandler bodySwapHandler,
        BodySwapQueryHandler bodySwapQueryHandler,
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
        // Old
        _handlers.Add(networkService.Connection.On<BodySwapQueryRequest, BodySwapQueryResponse>(HubMethod.BodySwapQuery, bodySwapQueryHandler.Handle));
        _handlers.Add(networkService.Connection.On<BodySwapAction>(HubMethod.BodySwap, bodySwapHandler.Handle));
        _handlers.Add(networkService.Connection.On<MoodlesAction>(HubMethod.Moodles, moodlesHandler.Handle));
        _handlers.Add(networkService.Connection.On<SpeakAction>(HubMethod.Speak, speakHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncOnlineStatusAction>(HubMethod.SyncOnlineStatus, syncOnlineStatusHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncPermissionsAction>(HubMethod.SyncPermissions, syncPermissionsHandler.Handle));
        _handlers.Add(networkService.Connection.On<TransformAction>(HubMethod.Transform, transformHandler.Handle));
        _handlers.Add(networkService.Connection.On<TwinningAction>(HubMethod.Twinning, twinningHandler.Handle));
        _handlers.Add(networkService.Connection.On<CustomizePlusAction>(HubMethod.CustomizePlus, customizePlusHandler.Handle));
        _handlers.Add(networkService.Connection.On<HypnosisAction>(HubMethod.Hypnosis, hypnosisHandler.Handle));
        
        // New
        _handlers.Add(networkService.Connection.On<EmoteForwardedRequest, ActionResult<Unit>>(HubMethod.Emote, emoteHandler.Handle));
    }

    public void Dispose()
    {
        foreach (var subscription in _handlers)
            subscription.Dispose();

        GC.SuppressFinalize(this);
    }
}