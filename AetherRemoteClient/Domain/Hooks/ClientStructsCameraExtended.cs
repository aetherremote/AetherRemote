using System.Runtime.InteropServices;

namespace AetherRemoteClient.Domain.Hooks;

[StructLayout(LayoutKind.Explicit)]
public unsafe struct ClientStructsCameraExtended
{
    [FieldOffset(0x0)] public nint* VirtualTable;
    [FieldOffset(0x60)] public float X;
    [FieldOffset(0x64)] public float Y;
    [FieldOffset(0x68)] public float Z;
    [FieldOffset(0x124)] public float Zoom;
    [FieldOffset(0x150)] public float InputDeltaH;
    [FieldOffset(0x154)] public float InputDeltaV;
}