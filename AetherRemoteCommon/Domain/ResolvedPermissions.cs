using AetherRemoteCommon.Domain.Enums.Permissions;
using MessagePack;

namespace AetherRemoteCommon.Domain;

[MessagePackObject]
public record ResolvedPermissions(
    [property: Key(0)] PrimaryPermissions Primary,
    [property: Key(1)] SpeakPermissions Speak,
    [property: Key(2)] ElevatedPermissions Elevated
);