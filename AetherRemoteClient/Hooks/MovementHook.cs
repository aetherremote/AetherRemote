using System;
using Dalamud.Hooking;

namespace AetherRemoteClient.Hooks;

/// <summary>
///     TODO
/// </summary>
public unsafe class MovementHook : IDisposable
{
    // Hook information
    private const string Signature = "E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D";
    private delegate void Delegate(void* self, float* horizontal, float* vertical, float* turn, byte* backwards, byte* a6, byte unknown);
    private readonly Hook<Delegate> _hook;
    
    // Values used to move the camera to a specific spot
    private float _h, _v, _t;
    private byte _b;
    
    /// <summary>
    ///     <inheritdoc cref="MovementHook"/>
    /// </summary>
    public MovementHook()
    {
        _hook = Plugin.GameInteropProvider.HookFromSignature<Delegate>(Signature, Detour);
    }

    public void Enable()
    {
        _h = 0;
        _v = 0;
        _t = 0;
        _b = 0;
        
        _hook.Enable();
    }
    public void Disable() => _hook.Disable();

    public void SetInput(float horizontal, float vertical, float turn, byte backwards)
    {
        _h = horizontal;
        _v = vertical;
        _t = turn;
        _b = backwards;
    }
    
    private void Detour(void* self, float* horizontal, float* vertical, float* turn, byte* backwards, byte* a6, byte unknown)
    {
        *horizontal = _h;
        *vertical = _v;
        *turn = _t;
        *backwards = _b;
        
        _hook.Original(self, horizontal, vertical, turn, backwards, a6, unknown);
    }

    public void Dispose()
    {
        _hook.Dispose();
        GC.SuppressFinalize(this);
    }
}