using MessagePack;

namespace AetherRemoteCommon.Domain.Network.Customize;

/// <summary>
///     Object containing the information to make a customize plus request to the server
/// </summary>
[MessagePackObject(true)]
public record CustomizeRequest : ActionRequest
{
    /// <summary>
    ///     The String64 representation of the JSON bone data for a CustomizePlus Profile
    /// </summary>
    public string Data { get; set; } = string.Empty;

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
    /// <param name="data">The String64 representation of the JSON bone data for a CustomizePlus Profile</param>
    public CustomizeRequest(List<string> targets, string data)
    {
        TargetFriendCodes = targets;
        Data = data;
    }
}