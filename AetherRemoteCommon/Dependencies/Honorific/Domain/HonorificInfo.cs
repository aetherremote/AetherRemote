using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.Dependencies.Honorific.Domain;

[MessagePackObject(keyAsPropertyName: true)]
public record HonorificInfo
{
    public string? Title { get; set; }
    public SerializableVector3? Color { get; set; }
    public SerializableVector3? Glow { get; set; }
}