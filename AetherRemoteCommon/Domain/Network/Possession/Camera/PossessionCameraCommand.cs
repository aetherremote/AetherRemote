using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Camera;

[MessagePackObject]
public record PossessionCameraCommand(
    string SenderFriendCode,
    [property: Key(1)] float HorizontalRotation,
    [property: Key(2)] float VerticalRotation,
    [property: Key(3)] float Zoom
) : ActionCommand(SenderFriendCode);