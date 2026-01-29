using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Dalamud.Hooking;

namespace AetherRemoteClient.Hooks;

/// <summary>
///     Hooks into the game's input system to record the changes in movement inputs
/// </summary>
public unsafe class MovementInputHook : IDisposable
{
    // Hook information
    private const string Signature = "E8 ?? ?? ?? ?? 80 7B 3E 00 48 8D 3D";
    private delegate void Delegate(void* self, float* horizontal, float* vertical, float* turn, byte* backwards, byte* a6, byte unknown);
    private readonly Hook<Delegate> _hook;

    // Control instance
    private readonly FFXIVClientStructs.FFXIV.Client.Game.Control.Control* _input;

    /// <summary>
    ///     Event fired when transitioning from one input value to another
    /// </summary>
    public event Func<float, float, float, byte, Task>? MovementInputValueChanged;
    
    /// <summary>
    ///     <inheritdoc cref="MovementInputHook"/>
    /// </summary>
    public MovementInputHook()
    {
        // Set up the detour
        _hook = Plugin.GameInteropProvider.HookFromSignature<Delegate>(Signature, Detour);
        
        // Copy an instance of the control struct so we can check if we are walking or not 
        _input = FFXIVClientStructs.FFXIV.Client.Game.Control.Control.Instance();
    }
    
    public void Enable() => _hook.Enable();
    public void Disable() => _hook.Disable();

    /// <summary>
    ///     When utilizing the <see cref="MovementLockHook"/>, the hook will set the value back to 0, 0, 0, 0 causing an alternating between
    ///     the real value and a row of all zeros. So we need to track essentially if we have two zeroes in a row (or any value before or answer a zero)
    /// </summary>
    /// <remarks>
    ///     The following is an example of examining the alternating behavior:<br/>
    ///     1, 0, 1, 0<br/>
    ///     0, 0, 0, 0<br/>
    ///     1, 0, 1, 0<br/>
    ///     0, 0, 0, 0<br/>
    ///     Etc...
    /// </remarks>
    private bool _previousDetourWasZero;

    // Values to determine the last message 
    private const float Tolerance = 0.00001F;
    private float _h, _v, _t;
    private byte _b;
    
    /// <summary>
    ///     Detour to report on the input data
    /// </summary>
    private void Detour(void* self, float* horizontal, float* vertical, float* turn, byte* backwards, byte* a6, byte unknown)
    {
        _hook.Original(self, horizontal, vertical, turn, backwards, a6, unknown);

        var h = SnapAwayFromIncrement(*horizontal);
        var v = SnapAwayFromIncrement(*vertical);
        var t = *turn;
        var b = *backwards;

        if (h is 0 && v is 0 && t is 0 && b is 0)
        {
            if (_previousDetourWasZero)
                TryInvokeValueChanged(h, v, t, b);
            
            _previousDetourWasZero = true;
        }
        else
        {
            _previousDetourWasZero = false;
            if (_input->IsWalking)
                TryInvokeValueChanged(h * 0.5f, v * 0.5f, t, b);
            else
                TryInvokeValueChanged(h, v, t, b);
        }
    }

    /// <summary>
    ///     Determines if a value is different from previously emitted values
    /// </summary>
    private void TryInvokeValueChanged(float h1, float v1, float t1, byte b1)
    {
        // If the values are the same as the last emitted message, don't send
        if (Math.Abs(_h - h1) < Tolerance && Math.Abs(_v - v1) < Tolerance && Math.Abs(_t - t1) < Tolerance && _b == b1)
            return;
        
        // Values are different, so save the new ones
        _h = h1;
        _v = v1;
        _t = t1;
        _b = b1;
        
        // Emit the event
        MovementInputValueChanged?.Invoke(_h, _v, _t, _b);
    }

    /// <summary>
    ///     Positions a value into brackets incrementing by 0.5 clamped between -1 and 1
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static float SnapAwayFromIncrement(float value)
    {
        return MathF.Sign(value) * MathF.Ceiling(MathF.Abs(value) * 2) * 0.5f;
    }

    public void Dispose()
    {
        _hook.Dispose();
        GC.SuppressFinalize(this);
    }
}