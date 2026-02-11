using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Customize;

/// <summary>
///     Object containing the information to make a customize plus request to the server
/// </summary>
[MessagePackObject]
public record CustomizeRequest(
    List<string> TargetFriendCodes,
    [property: Key(1)] byte[] JsonBoneDataBytes,
    [property: Key(2)] bool Additive
) : ActionRequest(TargetFriendCodes);