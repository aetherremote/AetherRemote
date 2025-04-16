using System.Numerics;
using MessagePack;

namespace AetherRemoteCommon.Domain;

/// <summary>
///     MessagePack doesn't natively support <see cref="Vector4"/>
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public struct SerializableVector4
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }
    public float W { get; set; }

    public override string ToString()
    {
        return $"{X}, {Y}, {Z}, {W}";
    }
    
    public static implicit operator Vector4(SerializableVector4 v) => new(v.X, v.Y, v.Z, v.W);
    public static implicit operator SerializableVector4(Vector4 v) => new() { X = v.X, Y = v.Y, Z = v.Z, W = v.W };
}