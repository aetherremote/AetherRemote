namespace AetherRemoteCommon.Domain.Network;

/// <summary>
/// Sent by the server to a target client to query data for a body swap request
/// </summary>
public struct BodySwapQueryRequest
{
    public string SenderFriendCode { get; set; }

    public BodySwapQueryRequest(string senderFriendCode)
    {
        SenderFriendCode = senderFriendCode;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapQueryRequest");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        return sb.ToString();
    }
}

public struct BodySwapQueryResponse
{
    public string? CharacterData { get; set; }

    public BodySwapQueryResponse(string? characterData)
    {
        CharacterData = characterData;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapQueryResponse");
        sb.AddVariable("CharacterData", CharacterData);
        return sb.ToString();
    }
}
