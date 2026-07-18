using AetherRemoteCommon.V2.Domain;
using MessagePack;

namespace AetherRemoteCommon.V2;

[MessagePackObject]
public record RoutedResponse<TPayload>(
    [property: Key(0)] RoutedResponseStatus Status,
    [property: Key(1)] TPayload? Payload = default
);
