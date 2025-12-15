using System.Numerics;
using MessagePack;

namespace AetherRemoteCommon.Domain;

/// <summary>
///     MessagePack doesn't natively support <see cref="Vector3"/>
/// </summary>
[MessagePackObject(keyAsPropertyName: true)]
public struct SerializableVector3
{
    public float X { get; set; }
    public float Y { get; set; }
    public float Z { get; set; }

    public override string ToString()
    {
        return $"{X}, {Y}, {Z}";
    }
    
    public static implicit operator Vector3(SerializableVector3 v) => new(v.X, v.Y, v.Z);
    public static implicit operator SerializableVector3(Vector3 v) => new() { X = v.X, Y = v.Y, Z = v.Z };
}