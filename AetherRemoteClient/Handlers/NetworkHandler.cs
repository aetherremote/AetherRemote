using System;
using System.Collections.Generic;
using AetherRemoteClient.Handlers.Network;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteCommon.Domain.Network.Emote;
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

    /// <summary>
    ///     <inheritdoc cref="NetworkHandler"/>
    /// </summary>
    public NetworkHandler(
        NetworkService networkService,
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
        // Responses Needed
        _handlers.Add(networkService.Connection.On<BodySwapForwardedRequest, ActionResult<Unit>>(HubMethod.BodySwap, bodySwapHandler.Handle));
        _handlers.Add(networkService.Connection.On<CustomizeForwardedRequest, ActionResult<Unit>>(HubMethod.CustomizePlus, customizePlusHandler.Handle));
        _handlers.Add(networkService.Connection.On<EmoteForwardedRequest, ActionResult<Unit>>(HubMethod.Emote, emoteHandler.Handle));
        _handlers.Add(networkService.Connection.On<HypnosisForwardedRequest, ActionResult<Unit>>(HubMethod.Hypnosis, hypnosisHandler.Handle));
        _handlers.Add(networkService.Connection.On<MoodlesForwardedRequest, ActionResult<Unit>>(HubMethod.Moodles, moodlesHandler.Handle));
        _handlers.Add(networkService.Connection.On<SpeakForwardedRequest, ActionResult<Unit>>(HubMethod.Speak, speakHandler.Handle));
        _handlers.Add(networkService.Connection.On<TransformForwardedRequest, ActionResult<Unit>>(HubMethod.Transform, transformHandler.Handle));
        _handlers.Add(networkService.Connection.On<TwinningForwardedRequest, ActionResult<Unit>>(HubMethod.Twinning, twinningHandler.Handle));
        
        // No Responses Needed
        _handlers.Add(networkService.Connection.On<SyncOnlineStatusForwardedRequest>(HubMethod.SyncOnlineStatus, syncOnlineStatusHandler.Handle));
        _handlers.Add(networkService.Connection.On<SyncPermissionsForwardedRequest>(HubMethod.SyncPermissions, syncPermissionsHandler.Handle));
    }

    public void Dispose()
    {
        foreach (var subscription in _handlers)
            subscription.Dispose();

        GC.SuppressFinalize(this);
    }
}