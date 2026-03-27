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
using AetherRemoteCommon.Domain.Network.Possession;
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
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.Handlers.Network;

public partial class NetworkHandler : IDisposable
{
    // Injected
    private readonly CustomizePlusService _customizePlusService;
    private readonly HonorificService _honorificService;
    private readonly MoodlesService _moodlesService;
    
    private readonly AccountService _accountService;
    private readonly ActionQueueService _actionQueueService;
    private readonly EmoteService _emoteService;
    private readonly FriendsListService _friendsListService;
    private readonly LogService _logService;
    private readonly PauseService _pauseService;
    private readonly StatusManager _statusManager;
    
    private readonly CharacterTransformationManager _characterTransformationManager;
    private readonly HypnosisManager _hypnosisManager;
    private readonly PossessionManager _possessionManager;
    private readonly SelectionManager _selectionManager;
    
    // Instantiated
    private readonly List<IDisposable> _handlers = [];
    
    public NetworkHandler(
        CustomizePlusService customizePlusService,
        HonorificService honorificService,
        MoodlesService moodlesService,
        
        AccountService accountService,
        ActionQueueService actionQueueService,
        EmoteService emoteService,
        FriendsListService friendsListService,
        LogService logService,
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
        
        // Common
        _handlers.Add(networkService.Connection.On<BodySwapCommand, ActionResult<Unit>>(HubMethod.BodySwap, HandleBodySwap));
        _handlers.Add(networkService.Connection.On<CustomizeCommand, ActionResult<Unit>>(HubMethod.CustomizePlus, HandleCustomizePlus));
        _handlers.Add(networkService.Connection.On<EmoteCommand, ActionResult<Unit>>(HubMethod.Emote, HandleEmoteCommand));
        _handlers.Add(networkService.Connection.On<HonorificCommand, ActionResult<Unit>>(HubMethod.Honorific, HandleHonorific));
        _handlers.Add(networkService.Connection.On<HypnosisCommand, ActionResult<Unit>>(HubMethod.Hypnosis, HandleHypnosis));
        _handlers.Add(networkService.Connection.On<HypnosisStopCommand, ActionResult<Unit>>(HubMethod.HypnosisStop, HandleHypnosisStop));
        _handlers.Add(networkService.Connection.On<MoodlesCommand, ActionResult<Unit>>(HubMethod.Moodles, HandleMoodles));
        _handlers.Add(networkService.Connection.On<SpeakCommand, ActionResult<Unit>>(HubMethod.Speak, HandleSpeak));
        _handlers.Add(networkService.Connection.On<SyncOnlineStatusCommand>(HubMethod.SyncOnlineStatus, HandleSyncOnlineStatus));
        _handlers.Add(networkService.Connection.On<SyncPermissionsCommand>(HubMethod.SyncPermissions, HandleSyncPermissions));
        _handlers.Add(networkService.Connection.On<TransformCommand, ActionResult<Unit>>(HubMethod.Transform, HandleTransform));
        _handlers.Add(networkService.Connection.On<TwinningCommand, ActionResult<Unit>>(HubMethod.Twinning, HandleTwinning));
        
        // Possession
        _handlers.Add(networkService.Connection.On<PossessionBeginCommand, PossessionResultEc>(HubMethod.Possession.Begin, HandlePossessionBegin));
        _handlers.Add(networkService.Connection.On<PossessionCameraCommand, PossessionResultEc>(HubMethod.Possession.Camera, HandlePossessionCamera));
        _handlers.Add(networkService.Connection.On<PossessionEndCommand, PossessionResultEc>(HubMethod.Possession.End, HandlePossessionEnd));
        _handlers.Add(networkService.Connection.On<PossessionMovementCommand, PossessionResultEc>(HubMethod.Possession.Movement, HandlePossessionMovement));
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