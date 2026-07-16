using MessagePack;

namespace AetherRemoteCommon.V2;

[MessagePackObject]
public record RoutedRequest<TPayload>(
    [property: Key(0)] string SenderFriendCode,
    [property: Key(1)] TPayload Payload
);