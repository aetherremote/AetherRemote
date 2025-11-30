using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Customize;

/// <summary>
///     Forwarded object containing the information to handle a customize plus request on a client
/// </summary>
[MessagePackObject(true)]
public record CustomizeForwardedRequest : ForwardedActionRequest
{
    /// <summary>
    ///     The String64 representation of the JSON bone data for a CustomizePlus Profile
    /// </summary>
    public string Data { get; set; } = string.Empty;

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
    /// <param name="data">The String64 representation of the JSON bone data for a CustomizePlus Profile</param>
    public CustomizeForwardedRequest(string sender, string data)
    {
        SenderFriendCode = sender;
        Data = data;
    }
}