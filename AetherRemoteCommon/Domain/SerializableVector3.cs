using System.Numerics;
using MessagePack;

namespace AetherRemoteCommon.Domain;

/// <summary>
///     MessagePack doesn't natively support <see cref="Vector3"/>
/// </summary>
[MessagePackObject]
public record SerializableVector3(
    [property: Key(0)] float X,
    [property: Key(1)] float Y,
    [property: Key(2)] float Z
)
{
    public static implicit operator Vector3(SerializableVector3 v) => new(v.X, v.Y, v.Z);
    public static implicit operator SerializableVector3(Vector3 v) => new(v.X, v.Y, v.Z);
}