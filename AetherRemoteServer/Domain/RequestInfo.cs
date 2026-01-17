using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteServer.Domain;


public record SpeakRequestInfo(string Method, SpeakPermissions2 Permissions, ActionCommand Request);
public record PrimaryRequestInfo(string Method, PrimaryPermissions2 Permissions, ActionCommand Request);