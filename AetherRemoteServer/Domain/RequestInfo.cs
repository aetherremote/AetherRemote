using AetherRemoteCommon.Domain.Enums.New;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.Speak;

namespace AetherRemoteServer.Domain;


public record SpeakRequestInfo(string Method, SpeakPermissions2 Permissions, ForwardedActionRequest Request);
public record PrimaryRequestInfo(string Method, PrimaryPermissions2 Permissions, ForwardedActionRequest Request);