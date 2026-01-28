using System;
using System.Threading.Tasks;
using AetherRemoteClient.Hooks;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.Begin;
using AetherRemoteCommon.Domain.Network.Possession.Camera;
using AetherRemoteCommon.Domain.Network.Possession.End;
using AetherRemoteCommon.Domain.Network.Possession.Movement;

namespace AetherRemoteClient.Managers;

public class PossessionManager : IDisposable
{
    private readonly CameraHook _cameraHook;
    private readonly CameraInputHook _cameraInputHook;
    private readonly CameraTargetHook _cameraTargetHook;
    private readonly MovementHook _movementHook;
    private readonly MovementInputHook _movementInputHook;
    private readonly MovementLockHook _movementLockHook;
    private readonly NetworkService _network;
    
    public PossessionManager(
        CameraHook cameraHook,
        CameraInputHook cameraInputHook,
        CameraTargetHook cameraTargetHook,
        MovementHook movementHook,
        MovementInputHook movementInputHook,
        MovementLockHook movementLockHook,
        NetworkService network)
    {
        _cameraHook = cameraHook;
        _cameraInputHook = cameraInputHook;
        _movementHook = movementHook;
        _movementInputHook = movementInputHook;
        _cameraTargetHook = cameraTargetHook;
        _movementLockHook = movementLockHook;
        _network = network;

        _network.Disconnected += OnDisconnect;
    }

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
            var response = await _network.InvokeAsync<PossessionBeginResponse>(HubMethod.Possession.Begin, request).ConfigureAwait(false);
            if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
            {
                Plugin.Log.Warning($"[PossessionManager.TryBeginPossession] Could not start a new session, {response.Response} {response.Result}");
                NotificationHelper.Warning("Unable to possess", $"Attempting to possess was {response.Response} and client's result was {response.Result}");
                return;
            }

            // Set the type, indicating we are in some sort of session
            Type = PossessionSessionType.Ghost;
            
            // Get the address of the game object for the character we are possessing
            var address = await Plugin.TryFindAddressByCharacter(response.CharacterName, response.CharacterWorld).ConfigureAwait(false);
            if (address == IntPtr.Zero)
                return;
            
            // Enable the hooks we want to, which are locking us in place, changing our target, and reading out inputs via "Listen Mode"
            _cameraTargetHook.Target(address);
            _movementLockHook.Enable();
            
            // Enable listening to the input events
            _cameraInputHook.Enable();
            _cameraInputHook.CameraInputValueChanged += OnCameraInputValueChanged;
            _movementInputHook.Enable();
            _movementInputHook.MovementInputValueChanged += OnMovementInputValueChanged;
            
            // Notification
            NotificationHelper.Info("You are possessing someone!", "You are now controlling your friend's movement and camera. To exit, visit the possession tab, or use /ar unpossess or /ar safeword.");
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PossessionManager.TryBeginPossession] {e}");
            Type = PossessionSessionType.None;
        }
    }

    /// <summary>
    ///     Attempts to become possessed by a "Ghost"
    /// </summary>
    public PossessionResultEc TryBecomePossessed()
    {
        if (Type is not PossessionSessionType.None)
        {
            Plugin.Log.Warning("[PossessionManager.TryBecomePossessed] Cannot become possessed if you are already in a session");
            return PossessionResultEc.AlreadyBeingPossessedOrPossessing;
        }
        
        // Set your type as host, meaning you are being possessed
        Type = PossessionSessionType.Host;
        
        // Enable hooks to lock you out of moving or controlling your camera
        _movementHook.Enable();
        _cameraHook.Enable();
        
        // Notification
        NotificationHelper.Info("You have been possessed!", "One of your friends is possessing you, and you are no longer able to control your movements or camera. To stop being possessed, go to the status menu or use /ar unpossess or /ar safeword");
        
        return PossessionResultEc.Success;
    }

    public void SetCameraDestination(float horizontal, float vertical, float zoom)
    {
        if (Type is not PossessionSessionType.Host)
        {
            Plugin.Log.Warning("[PossessionManager.SetCameraDestination] Cannot set destination if you are not possessed");
            return;
        }

        _cameraHook.SetTarget(horizontal, vertical, zoom);
    }

    public void SetMovementDirection(float horizontal, float vertical, float turn, byte backwards)
    {
        if (Type is not PossessionSessionType.Host)
        {
            Plugin.Log.Warning("[PossessionManager.SetMovementDirection] Cannot set direction if you are not possessed");
            return;
        }
        
        _movementHook.SetInput(horizontal, vertical, turn, backwards);
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
        
        // Disable all the hooks for possessing (both ghost and host)
        DisableAll();
        
        // Mark that we are no longer in a session
        Type = PossessionSessionType.None;

        var request = new PossessionEndRequest();
        try
        {
            var response = await _network.InvokeAsync<PossessionResponse>(HubMethod.Possession.End, request).ConfigureAwait(false);
            if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
            {
                Plugin.Log.Warning($"[PossessionManager.TryEndPossession] Could not end session {response.Response} {response.Result}");
                return;
            }
            
            // Notification
            NotificationHelper.Info("Possession ended", string.Empty);
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
            var response = await _network.InvokeAsync<PossessionResponse>(HubMethod.Possession.Camera, request).ConfigureAwait(false);
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
            var response = await _network.InvokeAsync<PossessionResponse>(HubMethod.Possession.Movement, request).ConfigureAwait(false);
            if (response.Response is not PossessionResponseEc.Success || response.Result is not PossessionResultEc.Success)
            {
                Plugin.Log.Warning($"[PossessionManager.SendMovementToServer] {response.Response} {response.Result}");
                if (response.Response is PossessionResponseEc.TooManyRequests)
                    NotificationHelper.Warning("Slow down!", "You're sending too many inputs!!");
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[PossessionManager.SendMovementToServer] {e}");
        }
    }

    private void DisableAll()
    {
        _cameraHook.Disable();
        _movementHook.Disable();
        _cameraTargetHook.Clear();
        _movementLockHook.Disable();
        _cameraInputHook.Disable();
        _cameraInputHook.CameraInputValueChanged -= OnCameraInputValueChanged;
        _movementInputHook.Disable();
        _movementInputHook.MovementInputValueChanged -= OnMovementInputValueChanged;
    }

    public void EndPossessing()
    {
        DisableAll();
        Type = PossessionSessionType.None;
        
        // Notification
        NotificationHelper.Info("Possession ended", string.Empty);
    }
    
    private Task OnDisconnect()
    {
        EndPossessing();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _network.Disconnected -= OnDisconnect;
        DisableAll();
        GC.SuppressFinalize(this);
    }
}