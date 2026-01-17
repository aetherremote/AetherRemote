using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Customize;

/// <summary>
///     Forwarded object containing the information to handle a customize plus request on a client
/// </summary>
[MessagePackObject]
public record CustomizeCommand(
    string SenderFriendCode,
    [property: Key(1)] byte[] JsonBoneDataBytes
) : ActionCommand(SenderFriendCode);