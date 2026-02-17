using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.Dependencies.Honorific.Domain;

[MessagePackObject(keyAsPropertyName: true)]
public record HonorificInfo2
{
    public string? Title { get; set; }
    public bool IsPrefix { get; set; }
    public SerializableVector3? Color { get; set; }
    public SerializableVector3? Glow { get; set; }
}

[MessagePackObject]
public record HonorificDto(
    [property: Key(0)] string Title, 
    [property: Key(1)] bool IsPrefix,
    [property: Key(2)] SerializableVector3? Color,
    [property: Key(3)] SerializableVector3? Glow
);