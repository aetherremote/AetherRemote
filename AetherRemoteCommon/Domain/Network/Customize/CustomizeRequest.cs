using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Customize;

/// <summary>
///     Object containing the information to make a customize plus request to the server
/// </summary>
[MessagePackObject(true)]
public record CustomizeRequest : ActionRequest
{
    /// <summary>
    ///     JSON representation of the bones in a Customize profile
    /// </summary>
    public byte[] JsonBoneDataBytes { get; set; } = [];

    /// <summary>
    ///     <inheritdoc cref="CustomizeRequest"/>
    /// </summary>
    public CustomizeRequest()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="CustomizeRequest"/>
    /// </summary>
    /// <param name="targets">The target friend codes</param>
    /// <param name="jsonBoneDataBytes">JSON representation of the bones in a Customize profile</param>
    public CustomizeRequest(List<string> targets, byte[] jsonBoneDataBytes)
    {
        TargetFriendCodes = targets;
        JsonBoneDataBytes = jsonBoneDataBytes;
    }
}