using MessagePack;

namespace AetherRemoteCommon.V2.Network.Relays;

[MessagePackObject]
public record EmotePayload(
    [property: Key(0)] string Emote,
    [property: Key(1)] bool DisplayLogMessage
);
