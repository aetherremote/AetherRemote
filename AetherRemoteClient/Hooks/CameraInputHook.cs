using System;
using System.Timers;
using AetherRemoteClient.Domain.Hooks;
using Dalamud.Hooking;

namespace AetherRemoteClient.Hooks;

/// <summary>
///     TODO
/// </summary>
public unsafe class CameraInputHook : IDisposable
{
    // Hook information
    private const string Signature = "48 8B C4 53 48 81 EC ?? ?? ?? ?? 44 0F 29 50 ??";
    private delegate void Delegate(ClientStructsCameraExtended* camera, int mode, float horizontal, float vertical);
    private readonly Hook<Delegate> _hook;
    //
    // Timer for a periodic check of the camera's position so we know where to move it to if any
    private readonly Timer _cameraPeriodicCheckTimer = new(2000);
    private bool _shouldCheckCamera;
    
    /// <summary>
    ///     Event fired when hook function is invoked
    /// </summary>
    public event Action<float, float, float>? CameraInputValueChanged;
    
    /// <summary>
    ///     <inheritdoc cref="CameraInputHook"/>
    /// </summary>
    public CameraInputHook()
    {
        _hook = Plugin.GameInteropProvider.HookFromSignature<Delegate>(Signature, Detour);
        _cameraPeriodicCheckTimer.AutoReset = true;
        _cameraPeriodicCheckTimer.Elapsed += OnCameraPeriodicCheckTimer;
    }

    /// <summary>
    ///     Set <see cref="_shouldCheckCamera"/> to be true, causing the next <see cref="Detour"/> call to attempt to emit the camera's new rotation values
    /// </summary>
    private void OnCameraPeriodicCheckTimer(object? sender, ElapsedEventArgs e)
    {
        _shouldCheckCamera = true;
    }

    public void Enable()
    {
        _hook.Enable();
        _cameraPeriodicCheckTimer.Stop();
        _cameraPeriodicCheckTimer.Start();
    }

    public void Disable()
    {
        _hook.Disable();
        _cameraPeriodicCheckTimer.Stop();
    }
    
    // Values to determine the last message 
    private const float Tolerance = 0.00001F;
    private float _h, _v, _z;
    
    /// <summary>
    ///     Detour to report on the input data
    /// </summary>
    private void Detour(ClientStructsCameraExtended* camera, int mode, float horizontal, float vertical)
    {
        _hook.Original(camera, mode, horizontal, vertical);

        // Only process if the trigger is set
        if (_shouldCheckCamera is false)
            return;
        
        // Reset the trigger again
        _shouldCheckCamera = false;
        
        var h = camera->CurrentHRotation;
        var v = camera->CurrentVRotation;
        var z = camera->Zoom;

        // If the values are the same as the last emitted message, don't send
        if (Math.Abs(_h - h) < Tolerance && Math.Abs(_v - v) < Tolerance && Math.Abs(_z - z) < Tolerance)
            return;

        // Values are different, so save the new ones
        _h = h;
        _v = v;
        _z = z;

        // Emit the event
        CameraInputValueChanged?.Invoke(_h, _v, _z);
    }
    
    public void Dispose()
    {
        _cameraPeriodicCheckTimer.Elapsed -= OnCameraPeriodicCheckTimer;
        _cameraPeriodicCheckTimer.Dispose();
        _hook.Dispose();
        GC.SuppressFinalize(this);
    }
}