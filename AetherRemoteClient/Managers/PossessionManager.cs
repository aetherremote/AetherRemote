using System;
using System.Threading.Tasks;
using AetherRemoteClient.Hooks;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using AetherRemoteCommon.Domain.Network.Possession.End;
using AetherRemoteCommon.Domain.Network.Possession.Movement;

namespace AetherRemoteClient.Managers;

public class PossessionManager(
    CameraHook cameraHook,
    CameraInputHook cameraInputHook, 
    CameraTargetHook cameraTargetHook, 
    MovementHook movementHook, 
    MovementInputHook movementInputHook,
    MovementLockHook movementLockHook, 
    NetworkService network) : IDisposable
{
    public PossessionSessionType Type { get; private set; } = PossessionSessionType.None;
    public enum PossessionSessionType
    {
        None,
        Host,
        Ghost
    }

    /// <summary>
    ///     Attempts to create a new possession session
    /// </summary>
    public async Task TryBeginPossession(string friendCode)
    {
        if (Type is not PossessionSessionType.None)
        {
            Plugin.Log.Warning("[PossessionManager.TryBeginPossession] Cannot start a new session while already in one");
            return;
        }

        var request = new PossessionBeginRequest(friendCode);
        try
        {
            var response = await network.InvokeAsync<PossessionBeginResponse>(HubMethod.Possession.Begin, request).ConfigureAwait(false);
            if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
            {
                Plugin.Log.Warning($"[PossessionManager.TryBeginPossession] Could not start a new session, {response.Response} {response.Result}");
                return;
            }

            // Set the type, indicating we are in some sort of session
            Type = PossessionSessionType.Ghost;
            
            // Get the address of the game object for the character we are possessing
            var address = await Plugin.TryFindAddressByCharacter(response.CharacterName, response.CharacterWorld).ConfigureAwait(false);
            if (address == IntPtr.Zero)
                return;
            
            // Enable the hooks we want to, which are locking us in place, changing our target, and reading out inputs via "Listen Mode"
            cameraTargetHook.Target(address);
            movementLockHook.Enable();
            
            // Enable listening to the input events
            cameraInputHook.Enable();
            cameraInputHook.CameraInputValueChanged += OnCameraInputValueChanged;
            movementInputHook.Enable();
            movementInputHook.MovementInputValueChanged += OnMovementInputValueChanged;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PossessionManager.TryBeginPossession] {e}");
            Type = PossessionSessionType.None;
        }
    }

    /// <summary>
    ///     Attempts to end a possession session
    /// </summary>
    public async Task TryEndPossession()
    {
        if (Type is PossessionSessionType.None)
        {
            Plugin.Log.Warning("[PossessionManager.TryEndPossession] Cannot end a session while you're not in one");
            return;
        }

        var request = new PossessionEndRequest();
        try
        {
            var response = await network.InvokeAsync<PossessionResponse>(HubMethod.Possession.Begin, request).ConfigureAwait(false);
            if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
            {
                Plugin.Log.Warning($"[PossessionManager.TryEndPossession] Could not end session {response.Response} {response.Result}");
                return;
            }
            
            // Disable all the hooks for possessing (both ghost and host)
            DisableAll();
            
            // Mark that we are no longer in a session
            Type = PossessionSessionType.None;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PossessionManager.TryEndPossession] {e}");
        }
    }
    
    /// <summary>
    ///     Handle event when the camera input value changes
    /// </summary>
    private void OnCameraInputValueChanged(float horizontalRotation, float verticalRotation, float zoom)
    {
        Plugin.Log.Verbose($"[PossessionManager.OnCameraInputValueChanged] {horizontalRotation} {verticalRotation} {zoom}");
        _ = SendCameraToServer(horizontalRotation, verticalRotation, zoom);
    }

    /// <summary>
    ///     Send to the server our camera rotation position and zoom
    /// </summary>
    private async Task SendCameraToServer(float horizontalRotation, float verticalRotation, float zoom)
    {
        if (Type is not PossessionSessionType.Ghost)
            Plugin.Log.Warning($"[PossessionManager.SendCameraToServer] Sending value as a non-ghost {Type}");
        
        var request = new PossessionCameraRequest(horizontalRotation, verticalRotation, zoom);
        try
        {
            var response = await network.InvokeAsync<PossessionResponse>(HubMethod.Possession.Camera, request).ConfigureAwait(false);
            if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
                Plugin.Log.Warning($"[PossessionManager.SendCameraToServer] {response.Response} {response.Result}");
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PossessionManager.SendCameraToServer] {e}");
        }
    }
    
    /// <summary>
    ///     Handle event when the movement input value changes
    /// </summary>
    private void OnMovementInputValueChanged(float horizontal, float vertical, float turn, byte backwards)
    {
        Plugin.Log.Verbose($"[PossessionManager.OnMovementInputValueChanged] {horizontal} {vertical} {turn} {backwards}");
        _ = SendMovementToServer(horizontal, vertical, turn, backwards);
    }

    /// <summary>
    ///     Send to the server our horizontal, vertical, turn rate, and moving backwards values
    /// </summary>
    private async Task SendMovementToServer(float horizontal, float vertical, float turn, byte backwards)
    {
        if (Type is not PossessionSessionType.Ghost)
            Plugin.Log.Warning($"[PossessionManager.SendMovementToServer] Sending value as a non-ghost {Type}");
        
        var request = new PossessionMovementRequest(horizontal, vertical, turn, backwards);
        try
        {
            var response = await network.InvokeAsync<PossessionResponse>(HubMethod.Possession.Movement, request).ConfigureAwait(false);
            if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
                Plugin.Log.Warning($"[PossessionManager.SendMovementToServer] {response.Response} {response.Result}");
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PossessionManager.SendMovementToServer] {e}");
        }
    }

    private void DisableAll()
    {
        cameraTargetHook.Clear();
        movementLockHook.Disable();
        cameraInputHook.Disable();
        cameraInputHook.CameraInputValueChanged -= OnCameraInputValueChanged;
        movementInputHook.Disable();
        movementInputHook.MovementInputValueChanged -= OnMovementInputValueChanged;
    }

    public void EndPossessing()
    {
        DisableAll();
    }

    public void Dispose()
    {
        DisableAll();
        GC.SuppressFinalize(this);
    }
}