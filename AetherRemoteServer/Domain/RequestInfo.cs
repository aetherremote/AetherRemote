using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network;

namespace AetherRemoteServer.Domain;


public record SpeakRequestInfo(string Method, SpeakPermissions2 Permissions, ForwardedActionRequest Request);
public record PrimaryRequestInfo(string Method, PrimaryPermissions2 Permissions, ForwardedActionRequest Request);