using System;
using AetherRemoteClient.Domain.Hooks;
using Dalamud.Hooking;
using FFXIVClientStructs.FFXIV.Client.Game.Control;

namespace AetherRemoteClient.Hooks;

public unsafe class CameraHook : IDisposable
{
    // Const
    private const float HorizontalRange = MathF.PI;
    
    private const float MinimumVerticalRotation = -85 * MathF.PI / 180f;
    private const float MaximumVerticalRotation = 45 * MathF.PI / 180f;
    private const float VerticalRange = MaximumVerticalRotation - MinimumVerticalRotation;
    
    private const float MinimumZoom = 1.5f;
    private const float MaximumZoom = 20f;
    private const float ZoomRange = MaximumZoom - MinimumZoom;
    
    private const float FloatTolerance = 0.0001F;
    private const float MaxSpeed = 0.01f;
    
    // Values used to move the camera to a specific spot
    private float _targetHorizontal, _targetVertical, _targetZoom;
    
    // Hook information
    private const string Signature = "48 8B C4 53 48 81 EC ?? ?? ?? ?? 44 0F 29 50 ??";
    private delegate void Delegate(ClientStructsCameraExtended* camera, int mode, float horizontal, float vertical);
    private readonly Hook<Delegate> _hook;
    
    /// <summary>
    ///     <inheritdoc cref="CameraHook"/>
    /// </summary>
    public CameraHook()
    {
        _hook = Plugin.GameInteropProvider.HookFromSignature<Delegate>(Signature, Detour);
    }

    public void Enable()
    {
        var camera = (ClientStructsCameraExtended*)CameraManager.Instance()->Camera;
        _targetHorizontal = camera->CurrentHRotation;
        _targetVertical = camera->CurrentVRotation;
        _targetZoom = camera->Zoom;
        
        _hook.Enable();
    }

    public void Disable()
    {
        _hook.Disable();
    }

    /// <summary>
    ///     Sets the target horizontal, vertical, and zoom values the camera should be moving towards
    /// </summary>
    public void SetTarget(float horizontal, float vertical, float zoom)
    {
        _targetHorizontal = horizontal;
        _targetVertical = vertical;
        _targetZoom = zoom;
    }
    
    /// <summary>
    ///     Detour to report on the input data
    /// </summary>
    private void Detour(ClientStructsCameraExtended* camera, int mode, float horizontal, float vertical)
    {
        _hook.Original(camera, mode, 0, 0);
        
        var h = camera->CurrentHRotation;
        var v = camera->CurrentVRotation;
        var z = camera->Zoom;

        if (Math.Abs(_targetHorizontal - h) < FloatTolerance && Math.Abs(_targetVertical - v) < FloatTolerance && Math.Abs(_targetZoom - z) < FloatTolerance)
            return;

        // Normalize all the values
        var deltaH = DeltaHorizontal(h, _targetHorizontal) / HorizontalRange;
        var deltaV = (_targetVertical - v) / VerticalRange;
        var deltaZ = (_targetZoom - z) / ZoomRange;
        
        var length = MathF.Sqrt(deltaH * deltaH + deltaV * deltaV + deltaZ * deltaZ);
        if (length < FloatTolerance)
            return;
        
        var scale = MathF.Min(1f, MaxSpeed / length);
        camera->CurrentHRotation += deltaH * scale * HorizontalRange;
        camera->CurrentVRotation += deltaV * scale * VerticalRange;
        camera->Zoom += deltaZ * scale * ZoomRange;
    }

    /// <summary>
    ///     Handles the nearest horizontal factoring in the 'wrapping' around the cube
    /// </summary>
    private static float DeltaHorizontal(float current, float target)
    {
        var delta = (target - current + MathF.PI) % (2f * MathF.PI);
        if (delta < 0f)
            delta += 2f * MathF.PI;

        return delta - MathF.PI;
    }
    
    public void Dispose()
    {
        _hook.Dispose();
        GC.SuppressFinalize(this);
    }
}