using System;
using AetherRemoteClient.Domain.Hooks;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using FFXIVClientStructs.FFXIV.Client.Game.Object;

namespace AetherRemoteClient.Hooks;

/// <summary>
///     TODO
/// </summary>
public unsafe class CameraTargetHook : IDisposable
{
    // Hook information
    private const int GetCameraTargetVirtualTableIndex = 17;
    private delegate GameObject* Delegate(ClientStructsCameraExtended* camera);
    private readonly Hook<Delegate> _hook;
    
    // Target to return when camera target function is invoked
    private nint? _target;
    
    /// <summary>
    ///     <inheritdoc cref="CameraTargetHook"/>
    /// </summary>
    public CameraTargetHook()
    {
        var camera = CameraManager.Instance()->GetActiveCamera();
        var extendedCamera = (ClientStructsCameraExtended*)camera;
        var address = extendedCamera->VirtualTable is null ? 0 : extendedCamera->VirtualTable[GetCameraTargetVirtualTableIndex];
        _hook = Plugin.GameInteropProvider.HookFromAddress<Delegate>(address, Detour);
    }

    public void Enable(nint address)
    {
        _target = address;
        _hook.Enable();
    }

    public void Disable()
    {
        _target = null;
        _hook.Disable();
    }
    
    private GameObject* Detour(ClientStructsCameraExtended* camera)
    {
        return _target is null ? _hook.Original(camera) : (GameObject*)_target;
    }

    public void Dispose()
    {
        _hook.Dispose();
        GC.SuppressFinalize(this);
    }
}