using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Camera;

[MessagePackObject]
public record PossessionCameraRequest(
    [property: Key(0)] float HorizontalRotation,
    [property: Key(1)] float VerticalRotation,
    [property: Key(2)] float Zoom
);