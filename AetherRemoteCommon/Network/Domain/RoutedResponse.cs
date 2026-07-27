using AetherRemoteCommon.Network.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain;

[MessagePackObject]
public record RoutedResponse<TPayload>(
    [property: Key(0)] RoutedResponseStatus Status,
    [property: Key(1)] TPayload? Payload = default
);
