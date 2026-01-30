using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using AetherRemoteCommon.Domain.Network.Possession.End;
using AetherRemoteCommon.Domain.Network.Possession.Movement;

namespace AetherRemoteClient.Managers.Possession;

public partial class PossessionManager
{
    // Variable to track our possession attempts
    private bool _attemptingPossession;
    
    /// <summary>
    ///     Possesses a target friend code
    /// </summary>
    public async Task<bool> Possess(Friend friend)
    {
        // Only start a new session if we aren't possessing or being possessed
        if (Possessing || Possessed)
            return false;

        // Get our current control scheme values
        if (await GameSettingsService.TryGetMoveMode().ConfigureAwait(false) is not { } moveMode)
            return false;
        
        // 'Lock' our state while we attempt to possess someone
        _attemptingPossession = true;
        
        // Begin network requesting
        var request = new PossessionBeginRequest(friend.FriendCode, moveMode);
        var response = await _network.InvokeAsync<PossessionBeginResponse>(HubMethod.Possession.Begin, request).ConfigureAwait(false);
        
        // Handle failures
        if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionManager.Possess] {response.Response} {response.Result}");
            NotificationHelper.Error("Possession Failed", $"Attempting to possess was {response.Response} and client's result was {response.Result}");
            _attemptingPossession = false;
            return false;
        }
        
        // Listen to events emitted by our hooks
        _cameraInputHook.CameraInputValueChanged += OnCameraInputValueChanged;
        _movementInputHook.MovementInputValueChanged += OnMovementInputValueChanged;
        
        // Enable the hooks relevant to use possessing someone
        _cameraInputHook.Enable();
        _movementInputHook.Enable();
        _movementLockHook.Enable();
        
        // Set our mode to be the Ghost mode, signifying we are possessing someone
        _possessionMode = PossessionMode.Ghost;
        
        // Reset our lock
        _attemptingPossession = false;
        
        // Now we can try to get the name of the character we are possessing to move our camera to them
        var address = await Plugin.TryFindAddressByCharacter(response.CharacterName, response.CharacterWorld).ConfigureAwait(false);
        if (address == IntPtr.Zero)
        {
            // If we didn't find the target, display a notification that you're possessing without them nearby
            NotificationHelper.Info("Remote Possession", "The person you are possessing is not nearby you, controlling them may be difficult.");
        }
        else
        {
            // We found the target, set the hooks to move our camera to them
            _cameraTargetHook.Enable(address);
        }

        // Send a success back
        return true;
    }
    
    /// <summary>
    ///     Unpossess from the previous target friend's body
    /// </summary>
    /// <param name="notifyOther">If the other person should be notified or not.</param>
    /// <remarks>Only call if you are the person possessing</remarks>
    public async Task<bool> Unpossess(bool notifyOther)
    {
        // Only handle cases where you are the possessor
        if (Possessing is false)
            return false;
        
        // Stop listening to events
        _cameraInputHook.CameraInputValueChanged -= OnCameraInputValueChanged;
        _movementInputHook.MovementInputValueChanged -= OnMovementInputValueChanged;
        
        // Disable all hooks, even if we did not enable them all just for safety
        _cameraInputHook.Disable();
        _cameraTargetHook.Disable();
        _movementInputHook.Disable();
        _movementLockHook.Disable();
        
        // Always mark ourselves as free now, even if network event fails for whatever reason
        _possessionMode = PossessionMode.None;

        // If we don't have to notify the other person, exit early
        if (notifyOther is false)
            return true;
        
        // NotificationHelper.Success("Unpossess Successful", string.Empty);
        
        // Notify the person we are possessing they are now free
        var request = new PossessionEndRequest();
        var response = await _network.InvokeAsync<PossessionResponse>(HubMethod.Possession.End, request).ConfigureAwait(false);
        if (response.Response is PossessionResponseEc.Success && response.Result is PossessionResultEc.Success)
            return true;
        
        // If there was a problem, just display a success with some info
        NotificationHelper.Info("Unpossess Partially Successful", $"You are no longer possessing your friend, but there were complications of the type {response.Response} {response.Result}");
        return false;
    }
    
    /// <summary>
    ///     Handle event when the camera input value changes
    /// </summary>
    private async Task OnCameraInputValueChanged(float horizontalRotation, float verticalRotation, float zoom)
    {
        var request = new PossessionCameraRequest(horizontalRotation, verticalRotation, zoom);
        var response = await _network.InvokeAsync<PossessionResponse>(HubMethod.Possession.Camera, request).ConfigureAwait(false);
        if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionManager.OnCameraInputValueChanged] {response.Response} {response.Result}");
            if (response.Result is PossessionResultEc.PossessionDesynchronization)
                if (await Unpossess(false).ConfigureAwait(false))
                    NotificationHelper.Info("Possession Ended - Desynchronization", string.Empty);
        }
    }
    
    /// <summary>
    ///     Handle event when the movement input value changes
    /// </summary>
    private async Task OnMovementInputValueChanged(float horizontal, float vertical, float turn, byte backwards)
    {
        var request = new PossessionMovementRequest(horizontal, vertical, turn, backwards);
        var response = await _network.InvokeAsync<PossessionResponse>(HubMethod.Possession.Movement, request).ConfigureAwait(false);
        if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
        {
            Plugin.Log.Warning($"[PossessionManager.OnMovementInputValueChanged] {response.Response} {response.Result}");
            if (response.Response is PossessionResponseEc.TooManyRequests)
                NotificationHelper.Warning("Slow down!", "You're sending too many inputs!!");
            
            if (response.Result is PossessionResultEc.PossessionDesynchronization)
                if (await Unpossess(false).ConfigureAwait(false))
                    NotificationHelper.Info("Possession Ended - Desynchronization", string.Empty);
        }
    }
}