using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Domain.Network.Possession.End;

namespace AetherRemoteClient.Managers.Possession;

public partial class PossessionManager
{
    // Store the original movement configuration values
    private uint? _previousMoveMode;
    
    /// <summary>
    ///     Become possessed by a friend
    /// </summary>
    public async Task<PossessionResultEc> BecomePossessed(uint moveMode)
    {
        // Don't become possessed if you are already possessing or being possessed by someone else
        if (Possessing || Possessed)
            return PossessionResultEc.AlreadyBeingPossessedOrPossessing;
        
        // Don't become possessed if we are attempting to possess someone else
        if (_attemptingPossession)
            return PossessionResultEc.AlreadyBeingPossessedOrPossessing;
        
        // Get our current move mode
        if (await GameSettingsService.TryGetMoveMode().ConfigureAwait(false) is not { } currentMoveMode)
            return PossessionResultEc.FailedToReadCharacterConfiguration;
        
        // Save our previous move mode
        _previousMoveMode = currentMoveMode;
        
        // Set to our possessor's move mode
        await GameSettingsService.SetMoveMode(moveMode).ConfigureAwait(false);
        
        // Enable hooks to lock you out of moving or controlling your camera
        _movementHook.Enable();
        _cameraHook.Enable();
        
        // Set your type as host, meaning you are being possessed
        _possessionMode = PossessionMode.Host;
        
        // Notification
        NotificationHelper.Info("You have been possessed!", string.Empty);

        // Return the successful action
        return PossessionResultEc.Success;
    }

    /// <summary>
    ///     Removes a ghost from your body
    /// </summary>
    /// <param name="notifyOther">If the other person should be notified or not.</param>
    /// <remarks>Only call if you are the person being possessed</remarks>
    public async Task<bool> Expel(bool notifyOther)
    {
        // Only handle cases where you are the possessed
        if (Possessed is false)
            return false;
        
        // Disable all hooks
        _cameraHook.Disable();
        _movementHook.Disable();

        // Revert the control scheme back to the original if a value is present
        if (_previousMoveMode is not null)
        {
            // Revert
            await GameSettingsService.SetMoveMode(_previousMoveMode.Value).ConfigureAwait(false);
            
            // Clear the previous move mode now that we are no longer being possessed
            _previousMoveMode = null;
        }
        
        // Always mark ourselves as free now, even if network event fails for whatever reason
        _possessionMode = PossessionMode.None;

        // If we don't have to notify the other person, exit early
        if (notifyOther is false)
            return true;
        
        // Notify the person we are possessing they are now free
        var request = new PossessionEndRequest();
        var response = await _network.InvokeAsync<PossessionResponse>(HubMethod.Possession.End, request).ConfigureAwait(false);
        if (response.Response is PossessionResponseEc.Success && response.Result is PossessionResultEc.Success)
            return true;
        
        // If there was a problem, just display a success with some info
        NotificationHelper.Info("Unpossessed Successful", $"You are no longer possessed by your friend, but there were complications of the type {response.Response} {response.Result}");
        return false;
    }
    
    /// <summary>
    ///     Sets the position the possessed player's camera should move towards
    /// </summary>
    public PossessionResultEc SetCameraDestination(float horizontal, float vertical, float zoom)
    {
        // If we're not possessed, some kind of desync occured
        if (Possessed is false)
            return PossessionResultEc.PossessionDesynchronization;

        // Set the values and return success
        _cameraHook.SetTarget(horizontal, vertical, zoom);
        return PossessionResultEc.Success;
    }
    
    /// <summary>
    ///     Sets the movement vector the possessed player should input
    /// </summary>
    public PossessionResultEc SetMovementDirection(float horizontal, float vertical, float turn, byte backwards)
    {
        // If we're not possessed, some kind of desync occured
        if (Possessed is false)
            return PossessionResultEc.PossessionDesynchronization;
        
        // Set the values and return success
        _movementHook.SetInput(horizontal, vertical, turn, backwards);
        return PossessionResultEc.Success;
    }
}