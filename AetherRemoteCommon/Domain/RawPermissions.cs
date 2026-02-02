using AetherRemoteCommon.Domain.Enums.Permissions;
using MessagePack;

namespace AetherRemoteCommon.Domain;

[MessagePackObject]
public record RawPermissions(
    [property: Key(0)] PrimaryPermissions2 PrimaryAllow,
    [property: Key(1)] PrimaryPermissions2 PrimaryDeny,
    [property: Key(2)] SpeakPermissions2 SpeakAllow,
    [property: Key(3)] SpeakPermissions2 SpeakDeny,
    [property: Key(4)] ElevatedPermissions ElevatedAllow,
    [property: Key(5)] ElevatedPermissions ElevatedDeny
);