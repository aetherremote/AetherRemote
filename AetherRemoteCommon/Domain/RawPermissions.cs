using AetherRemoteCommon.Domain.Enums.Permissions;
using MessagePack;

namespace AetherRemoteCommon.Domain;

[MessagePackObject]
public record RawPermissions(
    [property: Key(0)] PrimaryPermissions PrimaryAllow,
    [property: Key(1)] PrimaryPermissions PrimaryDeny,
    [property: Key(2)] SpeakPermissions SpeakAllow,
    [property: Key(3)] SpeakPermissions SpeakDeny,
    [property: Key(4)] ElevatedPermissions ElevatedAllow,
    [property: Key(5)] ElevatedPermissions ElevatedDeny
);