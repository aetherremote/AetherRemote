using System;
using System.Collections.Generic;
using AetherRemoteClient.Handlers.Network;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Customize;
using AetherRemoteCommon.V2.Domain.Network.Emote;
using AetherRemoteCommon.V2.Domain.Network.Hypnosis;
using AetherRemoteCommon.V2.Domain.Network.Moodles;
using AetherRemoteCommon.V2.Domain.Network.Speak;
using AetherRemoteCommon.V2.Domain.Network.SyncOnlineStatus;
using AetherRemoteCommon.V2.Domain.Network.SyncPermissions;
using AetherRemoteCommon.V2.Domain.Network.Transform;
using AetherRemoteCommon.V2.Domain.Network.Twinning;
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
        _handlers.Add(networkService.Connection.On<MoodlesForwardedRequest>(HubMethod.Moodles, moodlesHandler.Handle));
        _handlers.Add(networkService.Connection.On<SpeakForwardedRequest>(HubMethod.Speak, speakHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncOnlineStatusForwardedRequest>(HubMethod.SyncOnlineStatus, syncOnlineStatusHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncPermissionsForwardedRequest>(HubMethod.SyncPermissions, syncPermissionsHandler.Handle));
        _handlers.Add(networkService.Connection.On<TransformForwardedRequest>(HubMethod.Transform, transformHandler.Handle));
        _handlers.Add(networkService.Connection.On<TwinningForwardedRequest>(HubMethod.Twinning, twinningHandler.Handle));
       
        _handlers.Add(networkService.Connection.On<HypnosisForwardedRequest>(HubMethod.Hypnosis, hypnosisHandler.Handle));
        
        // New
        _handlers.Add(networkService.Connection.On<CustomizeForwardedRequest, ActionResult<Unit>>(HubMethod.CustomizePlus, customizePlusHandler.Handle));
        _handlers.Add(networkService.Connection.On<EmoteForwardedRequest, ActionResult<Unit>>(HubMethod.Emote, emoteHandler.Handle));
    }

    public void Dispose()
    {
        foreach (var subscription in _handlers)
            subscription.Dispose();

        GC.SuppressFinalize(this);
    }
}