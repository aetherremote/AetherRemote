using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Customize;

/// <summary>
///     Forwarded object containing the information to handle a customize plus request on a client
/// </summary>
[MessagePackObject(true)]
public record CustomizeForwardedRequest : ForwardedActionRequest
{
    /// <summary>
    ///     JSON representation of the bones in a Customize profile
    /// </summary>
    public byte[] JsonBoneDataBytes { get; set; } = [];

    /// <summary>
    ///     <inheritdoc cref="CustomizeRequest"/>
    /// </summary>
    public CustomizeForwardedRequest()
    {
    }

    /// <summary>
    ///     <inheritdoc cref="CustomizeRequest"/>
    /// </summary>
    /// <param name="sender">The sender of the request's friend code</param>
    /// <param name="jsonBoneDataBytes">JSON representation of the bones in a Customize profile</param>
    public CustomizeForwardedRequest(string sender, byte[] jsonBoneDataBytes)
    {
        SenderFriendCode = sender;
        JsonBoneDataBytes = jsonBoneDataBytes;
    }
}