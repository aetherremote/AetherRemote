using AetherRemoteCommon.Network.Enums;
using MessagePack;

namespace AetherRemoteCommon.Network.Domain;

[MessagePackObject]
public record Response<TPayload>(
    [property: Key(0)] ResponseStatus Status,
    [property: Key(1)] Dictionary<string, RoutedResponseStatus> Results,
    [property: Key(2)] TPayload? Payload = default
);
