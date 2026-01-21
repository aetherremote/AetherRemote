using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Camera;

[MessagePackObject]
public record PossessionCameraRequest(
    [property: Key(0)] float Zoom,
    [property: Key(1)] float X,
    [property: Key(2)] float Y,
    [property: Key(3)] float Z
);