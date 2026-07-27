using MessagePack;

namespace AetherRemoteCommon.Network.Domain.Payloads;

[MessagePackObject]
public record EmotePayload(
    [property: Key(0)] string Emote,
    [property: Key(1)] bool DisplayLogMessage
);
