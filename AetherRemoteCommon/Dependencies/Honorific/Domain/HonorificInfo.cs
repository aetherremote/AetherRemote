using AetherRemoteCommon.Domain;
using MessagePack;

namespace AetherRemoteCommon.Dependencies.Honorific.Domain;

[MessagePackObject]
public record HonorificDto(
    [property: Key(0)] string Title, 
    [property: Key(1)] bool IsPrefix,
    [property: Key(2)] SerializableVector3? Color,
    [property: Key(3)] SerializableVector3? Glow
);