using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Possession.Camera;

[MessagePackObject]
public record PossessionCameraCommand(
    string SenderFriendCode,
    [property: Key(1)] float Zoom,
    [property: Key(2)] float X,
    [property: Key(3)] float Y,
    [property: Key(4)] float Z
) : ActionCommand(SenderFriendCode);