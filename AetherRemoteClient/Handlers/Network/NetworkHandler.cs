using System;
using System.Collections.Generic;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Honorific;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using AetherRemoteCommon.Domain.Network.HypnosisStop;
using AetherRemoteCommon.Domain.Network.Moodles;
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using AetherRemoteCommon.Domain.Network.Possession.End;
using AetherRemoteCommon.Domain.Network.Possession.Movement;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Domain.Network.SyncOnlineStatus;
using AetherRemoteCommon.Domain.Network.SyncPermissions;
using AetherRemoteCommon.Domain.Network.Transform;
using AetherRemoteCommon.Domain.Network.Twinning;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler : IDisposable
{
    // Injected
    
    private readonly AccountService _accountService;
    private readonly ActionQueueService _actionQueueService;
    private readonly CustomizePlusService _customizePlusService;
    private readonly EmoteService _emoteService;
    private readonly FriendsListService _friendsListService;
    private readonly HonorificService _honorificService;
    private readonly LogService _logService;
    private readonly MoodlesService _moodlesService;
    private readonly PauseService _pauseService;
    private readonly StatusManager _statusManager;
    
    private readonly CharacterTransformationManager _characterTransformationManager;
    private readonly HypnosisManager _hypnosisManager;
    private readonly PossessionManager _possessionManager;
    private readonly SelectionManager _selectionManager;
    
    // Instantiated
    private readonly List<IDisposable> _handlers = [];
    
    public NetworkHandler(
        AccountService accountService,
        ActionQueueService actionQueueService,
        CustomizePlusService customizePlusService,
        EmoteService emoteService,
        FriendsListService friendsListService,
        HonorificService honorificService,
        LogService logService,
        MoodlesService moodlesService,
        NetworkService networkService,
        PauseService pauseService,
        StatusManager statusManager,
        
        CharacterTransformationManager characterTransformationManager,
        HypnosisManager hypnosisManager,
        PossessionManager possessionManager,
        SelectionManager selectionManager)
    {
        // Injected
        _customizePlusService = customizePlusService;
        _honorificService = honorificService;
        _moodlesService = moodlesService;
        
        _accountService = accountService;
        _actionQueueService = actionQueueService;
        _emoteService = emoteService;
        _friendsListService = friendsListService;
        _logService = logService;
        _pauseService = pauseService;
        _statusManager = statusManager;
        
        _characterTransformationManager = characterTransformationManager;
        _hypnosisManager = hypnosisManager;
        _possessionManager = possessionManager;
        _selectionManager = selectionManager;
        
        // Synchronous Handlers
        _handlers.Add(networkService.ListenFunc<EmoteCommand>(HubMethod.Emote, HandleEmoteCommand));
        _handlers.Add(networkService.ListenFunc<SpeakCommand>(HubMethod.Speak, HandleSpeak));
        
        // Asynchronous Handlers
        _handlers.Add(networkService.ListenAction<SyncOnlineStatusCommand>(HubMethod.SyncOnlineStatus, HandleSyncOnlineStatus));
        _handlers.Add(networkService.ListenAction<SyncPermissionsCommand>(HubMethod.SyncPermissions, HandleSyncPermissions));
        _handlers.Add(networkService.ListenFuncAsync<BodySwapCommand>(HubMethod.BodySwap, HandleBodySwap));
        _handlers.Add(networkService.ListenFuncAsync<CustomizeCommand>(HubMethod.CustomizePlus, HandleCustomizePlus));
        _handlers.Add(networkService.ListenFuncAsync<HonorificCommand>(HubMethod.Honorific, HandleHonorific));
        _handlers.Add(networkService.ListenFuncAsync<HypnosisCommand>(HubMethod.Hypnosis, HandleHypnosis));
        _handlers.Add(networkService.ListenFuncAsync<HypnosisStopCommand>(HubMethod.HypnosisStop, HandleHypnosisStop));
        _handlers.Add(networkService.ListenFuncAsync<MoodlesCommand>(HubMethod.Moodles, HandleMoodles));
        _handlers.Add(networkService.ListenFuncAsync<TransformCommand>(HubMethod.Transform, HandleTransform));
        _handlers.Add(networkService.ListenFuncAsync<TwinningCommand>(HubMethod.Twinning, HandleTwinning));
        
        // Synchronous Possession Handlers
        _handlers.Add(networkService.ListenPossession<PossessionCameraCommand>(HubMethod.Possession.Camera, HandlePossessionCamera));
        _handlers.Add(networkService.ListenPossession<PossessionMovementCommand>(HubMethod.Possession.Movement, HandlePossessionMovement));
        
        // Asynchronous Possession Handlers
        _handlers.Add(networkService.ListenPossessionAsync<PossessionBeginCommand>(HubMethod.Possession.Begin, HandlePossessionBegin));
        _handlers.Add(networkService.ListenPossessionAsync<PossessionEndCommand>(HubMethod.Possession.End, HandlePossessionEnd));
    }

    private ActionResult<Friend> TryGetFriendWithCorrectPermissions(string operation, string friendCode, ResolvedPermissions permissions)
    {
        // Not friends
        if (_friendsListService.Get(friendCode) is not { } friend)
        {
            _logService.NotFriends(operation, friendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientNotFriends);
        }
        
        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            _logService.SafeMode(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientInSafeMode);
        }

        // Friend Paused
        if (_pauseService.IsFriendPaused(friend.FriendCode))
        {
            _logService.FriendPaused(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasSenderPaused);
        }

        // Feature Paused
        if (_pauseService.IsFeaturePaused(permissions))
        {
            _logService.FeaturePaused(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasFeaturePaused);
        }
        
        // Resolve
        var resolved = PermissionResolver.Resolve(_accountService.GlobalPermissions, friend.PermissionsGrantedToFriend);
        
        // Test Primary Permissions
        if ((resolved.Primary & permissions.Primary) != permissions.Primary)
        {
            _logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }
        
        // Test Speak Permissions
        if ((resolved.Speak & permissions.Speak) != permissions.Speak)
        {
            _logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }
        
        // Test Elevated Permissions
        if ((resolved.Elevated & permissions.Elevated) != permissions.Elevated)
        {
            _logService.LackingPermissions(operation, friend.NoteOrFriendCode);
            return ActionResultBuilder.Fail<Friend>(ActionResultEc.ClientHasNotGrantedSenderPermissions);
        }
        
        return ActionResultBuilder.Ok(friend);
    }
    
    /// <summary>
    ///     Shared between Body Swap & Twinning
    /// </summary>
    private void UpdateStatusServicePostBodySwapOrTwinning(Friend applier, CharacterAttributes attributes)
    {
        if ((attributes & CharacterAttributes.PenumbraMods) is CharacterAttributes.PenumbraMods)
            _statusManager.SetGlamourerPenumbra(applier);
        
        if ((attributes & CharacterAttributes.CustomizePlus) is CharacterAttributes.CustomizePlus)
            _statusManager.SetCustomizePlus(applier);
        
        if ((attributes & CharacterAttributes.Honorific) is CharacterAttributes.Honorific)
            _statusManager.SetHonorific(applier);
    }
    
    public void Dispose()
    {
        foreach (var handler in _handlers)
            handler.Dispose();
        
        GC.SuppressFinalize(this);
    }
}