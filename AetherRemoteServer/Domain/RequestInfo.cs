using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteServer.Domain;


public record SpeakRequestInfo(string Method, SpeakPermissions Permissions, ActionCommand Request);
public record PrimaryRequestInfo(string Method, PrimaryPermissions Permissions, ActionCommand Request);