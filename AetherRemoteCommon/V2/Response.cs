using AetherRemoteCommon.V2.Domain;
using MessagePack;

namespace AetherRemoteCommon.V2;

[MessagePackObject]
public record Response<TPayload>(
    [property: Key(0)] ResponseStatus Status,
    [property: Key(1)] Dictionary<string, RoutedResponseStatus> Results,
    [property: Key(2)] TPayload? Payload = default
);