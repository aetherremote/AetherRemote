using System;
using AetherRemoteClient.Domain.Hooks;
using Dalamud.Hooking;

namespace AetherRemoteClient.Hooks;

/// <summary>
///     TODO
/// </summary>
public unsafe class CameraHook : IDisposable
{
    // Hook information
    private const string Signature = "48 8B C4 53 48 81 EC ?? ?? ?? ?? 44 0F 29 50 ??";
    private delegate void Delegate(ClientStructsCameraExtended* camera, int mode, float horizontal, float vertical);
    private readonly Hook<Delegate> _hook;

    /// <summary>
    ///     Event fired when hook function is invoked
    /// </summary>
    public event Action<float, float, float>? Hook;
    
    /// <summary>
    ///     <inheritdoc cref="CameraHook"/>
    /// </summary>
    public CameraHook()
    {
        _hook = Plugin.GameInteropProvider.HookFromSignature<Delegate>(Signature, Detour);
    }
    
    public void Enable() => _hook.Enable();
    public void Disable() => _hook.Disable();
    
    private void Detour(ClientStructsCameraExtended* camera, int mode, float horizontal, float vertical)
    {
        _hook.Original(camera, mode, horizontal, vertical);

        // If this has values in it, we're listening, so we shouldn't be invoking any events anyway to prevent circular loops
        if (false)
        {
            /*
            camera->InputDeltaH = packet.HorizontalDelta;
            camera->InputDeltaV = packet.VerticalDelta;
            camera->Zoom = packet.Zoom;
            */
        }
        else
        {
            // TODO: Missing zoom constraint, meaning that if we just zoom in and out nothing happens
            if (horizontal is 0 && vertical is 0) return;
            
            Hook?.Invoke(horizontal, vertical, camera->Zoom);
        }
    }
    
    public void Dispose()
    {
        _hook.Dispose();
        GC.SuppressFinalize(this);
    }
}