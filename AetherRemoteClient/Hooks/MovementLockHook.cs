namespace AetherRemoteClient.Hooks;

/// <summary>
///     TODO
/// </summary>
public unsafe class MovementLockHook
{
    // Hook information
    private const string Signature = "F3 0F 10 05 ?? ?? ?? ?? 0F 2E C7";
    private readonly nint _forceDisableMovementPointer;
    private ref int ForceDisableMovement => ref *(int*)_forceDisableMovementPointer;

    /// <summary>
    ///     <inheritdoc cref="MovementLockHook"/>
    /// </summary>
    public MovementLockHook()
    {
        if (Plugin.SigScanner.TryGetStaticAddressFromSig(Signature, out var address))
            _forceDisableMovementPointer = address + 4;
        
        // TODO: If this number is 0, logging?
    }

    public void Enable() => ForceDisableMovement = 1;
    public void Disable() => ForceDisableMovement = 0;
}