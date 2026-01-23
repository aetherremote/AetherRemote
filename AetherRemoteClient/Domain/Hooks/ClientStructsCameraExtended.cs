using System.Runtime.InteropServices;

namespace AetherRemoteClient.Domain.Hooks;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ClientStructsCameraExtended
{
    [FieldOffset(0x0)] public nint* VirtualTable;
    [FieldOffset(0x124)] public float Zoom;
    [FieldOffset(0x140)] public float CurrentHRotation;
    [FieldOffset(0x144)] public float CurrentVRotation;
}