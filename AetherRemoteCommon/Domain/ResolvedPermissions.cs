using AetherRemoteCommon.Domain.Enums.Permissions;
using MessagePack;

namespace AetherRemoteCommon.Domain;

[MessagePackObject]
public record ResolvedPermissions(
    [property: Key(0)] PrimaryPermissions2 Primary,
    [property: Key(1)] SpeakPermissions2 Speak,
    [property: Key(2)] ElevatedPermissions Elevated
);