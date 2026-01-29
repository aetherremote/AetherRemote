using System;
using System.Threading.Tasks;
using AetherRemoteClient.Hooks;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.Managers.Possession;

/// <summary>
///     Manages the state of a possessed player
/// </summary>
public partial class PossessionManager : IDisposable
{
    // Injected
    private readonly CameraHook _cameraHook;
    private readonly CameraInputHook _cameraInputHook;
    private readonly CameraTargetHook _cameraTargetHook;
    private readonly MovementHook _movementHook;
    private readonly MovementInputHook _movementInputHook;
    private readonly MovementLockHook _movementLockHook;
    private readonly NetworkService _network;
    
    // What 'mode' we're in, providing clarity on how we will interact with the different hooks
    private PossessionMode _possessionMode = PossessionMode.None;

    /// <summary>
    ///     If you are being possessed
    /// </summary>
    public bool Possessed => _possessionMode is PossessionMode.Host;
    
    /// <summary>
    ///     If you are doing the possessing
    /// </summary>
    public bool Possessing => _possessionMode is PossessionMode.Ghost;
    
    /// <summary>
    ///     <inheritdoc cref="PossessionManager"/>
    /// </summary>
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

    /// <summary>
    ///     Helper method to determine how the possession should be stopped
    /// </summary>
    /// <param name="silent">If the other person should be notified or not.</param>
    public async Task EndAllParanormalActivity(bool silent)
    {
        if (Possessed)
            await Expel(silent).ConfigureAwait(false);
        
        if (Possessing)
            await Unpossess(silent).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     When we disconnect from the server
    /// </summary>
    private async Task OnDisconnect()
    {
        await EndAllParanormalActivity(true).ConfigureAwait(false);
    }

    /// <summary>
    ///     Standard dispose pattern
    /// </summary>
    public void Dispose()
    {
        _network.Disconnected -= OnDisconnect;
        GC.SuppressFinalize(this);
    }
    
    /// <summary>
    ///     Enum detailing the internal state of our possession status
    /// </summary>
    private enum PossessionMode
    {
        None,
        Host,
        Ghost
    }
}