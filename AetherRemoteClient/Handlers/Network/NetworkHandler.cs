using System;
using System.Collections.Generic;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;
using AetherRemoteClient.Services.Dependencies;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler : IDisposable
{
    // Injected
    private readonly ActionQueueService _actionQueueService;
    private readonly ActiveSessionService _activeSessionService;
    private readonly ConfigurationService _configurationService;
    private readonly CustomizePlusService _customizePlusService;
    private readonly EmoteService _emoteService;
    private readonly FriendsListService _friendsListService;
    private readonly HonorificService _honorificService;
    private readonly LogService _logService;
    private readonly MoodlesService _moodlesService;
    private readonly PauseService _pauseService;
    private readonly StatusService _statusService;
    
    private readonly CharacterTransformationManager _characterTransformationManager;
    private readonly HypnosisManager _hypnosisManager;
    private readonly SelectionManager _selectionManager;
    
    // Instantiated
    private readonly List<IDisposable> _handlers = [];
    
    public NetworkHandler(
        ActionQueueService actionQueueService,
        ActiveSessionService activeSessionService,
        ConfigurationService configurationService,
        CustomizePlusService customizePlusService,
        EmoteService emoteService,
        FriendsListService friendsListService,
        HonorificService honorificService,
        LogService logService,
        MoodlesService moodlesService,
        NetworkService networkService,
        PauseService pauseService,
        
        CharacterTransformationManager characterTransformationManager,
        HypnosisManager hypnosisManager,
        SelectionManager selectionManager,
        StatusService statusService)
    {
        _actionQueueService = actionQueueService;
        _activeSessionService = activeSessionService;
        _configurationService = configurationService;
        _customizePlusService = customizePlusService;
        _emoteService = emoteService;
        _friendsListService = friendsListService;
        _honorificService = honorificService;
        _logService = logService;
        _moodlesService = moodlesService;
        _pauseService = pauseService;
        
        _characterTransformationManager = characterTransformationManager;
        _hypnosisManager = hypnosisManager;
        _selectionManager = selectionManager;
        _statusService = statusService;
        
        // Messages - Things the server just updates us on
        _handlers.Add(networkService.Listen<Message<SyncOnlineStatusPayload>>(HubMethod.SyncOnlineStatus, HandleSyncOnlineStatus));
        _handlers.Add(networkService.Listen<Message<SyncPermissionsPayload>>(HubMethod.SyncPermissions, HandleSyncPermissions));
        
        // Handles - Requests from other clients we are expected to act upon
        _handlers.Add(networkService.Listen<EmotePayload, NoPayload>(HubMethod.Emote, HandleEmoteCommand));
        _handlers.Add(networkService.Listen<SpeakPayload, NoPayload>(HubMethod.Speak, HandleSpeak));
        _handlers.Add(networkService.ListenAsync<BodySwapRoutedPayload, NoPayload>(HubMethod.BodySwap, HandleBodySwap));
        _handlers.Add(networkService.ListenAsync<CustomizePlusPayload, NoPayload>(HubMethod.CustomizePlus, HandleCustomizePlus));
        _handlers.Add(networkService.ListenAsync<HonorificPayload, NoPayload>(HubMethod.Honorific, HandleHonorific));
        _handlers.Add(networkService.ListenAsync<HypnosisPayload, NoPayload>(HubMethod.Hypnosis, HandleHypnosis));
        _handlers.Add(networkService.ListenAsync<HypnosisStopPayload, NoPayload>(HubMethod.HypnosisStop, HandleHypnosisStop));
        _handlers.Add(networkService.ListenAsync<MimicryPayload, NoPayload>(HubMethod.Mimicry, HandleMimicry));
        _handlers.Add(networkService.ListenAsync<MoodlesPayload, NoPayload>(HubMethod.Moodles, HandleMoodles));
        _handlers.Add(networkService.ListenAsync<TransformationPayload, NoPayload>(HubMethod.Transform, HandleTransform));
        _handlers.Add(networkService.ListenAsync<TwinningPayload, NoPayload>(HubMethod.Twinning, HandleTwinning));
    }

    private RoutedResponseStatus? GetValidationError(string operation, Friend friend, ResolvedPermissions permissions)
    {
        if (_configurationService.SafeMode)
        {
            _logService.SafeMode(operation, friend.NoteOrFriendCode);
            return RoutedResponseStatus.SafeMode;
        }
        
        if (_pauseService.IsFriendPaused(friend.FriendCode))
        {
            _logService.FriendPaused(operation, friend.NoteOrFriendCode);
            return RoutedResponseStatus.Paused;
        }
        
        if (_pauseService.IsFeaturePaused(permissions))
        {
            _logService.FeaturePaused(operation, friend.NoteOrFriendCode);
            return RoutedResponseStatus.Paused;
        }

        if (_activeSessionService.GlobalPermissions is null)
        {
            Plugin.Log.Error("[NetworkHandler.BasicValidation] GlobalPermissions not set.");
            return RoutedResponseStatus.Unknown;
        }
        
        var resolved = PermissionResolver.Resolve(_activeSessionService.GlobalPermissions, friend.PermissionsGrantedToFriend);
        if ((resolved.Primary & permissions.Primary) != permissions.Primary || 
            (resolved.Speak & permissions.Speak) != permissions.Speak || 
            (resolved.Elevated & permissions.Elevated) != permissions.Elevated)
        {
            _logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return RoutedResponseStatus.LackingPermissions;
        }

        return null;
    }
    
    /// <summary>
    ///     Shared between Body Swap & Twinning
    /// </summary>
    private void UpdateStatusServicePostBodySwapOrTwinning(Friend applier, CharacterAttributes attributes)
    {
        if ((attributes & CharacterAttributes.PenumbraMods) is CharacterAttributes.PenumbraMods)
            _statusService.SetGlamourerPenumbra(applier);
        
        if ((attributes & CharacterAttributes.CustomizePlus) is CharacterAttributes.CustomizePlus)
            _statusService.SetCustomizePlus(applier);
        
        if ((attributes & CharacterAttributes.Honorific) is CharacterAttributes.Honorific)
            _statusService.SetHonorific(applier);
    }
    
    public void Dispose()
    {
        foreach (var handler in _handlers)
            handler.Dispose();
        
        GC.SuppressFinalize(this);
    }
}