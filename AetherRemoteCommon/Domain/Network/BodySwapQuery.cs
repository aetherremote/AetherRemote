namespace AetherRemoteCommon.Domain.Network;

/// <summary>
/// Sent by the server to a target client to query data for a body swap request
/// </summary>
public struct BodySwapQueryRequest
{
    public string SenderFriendCode { get; set; }
    public bool SwapMods { get; set; }

    public BodySwapQueryRequest(string senderFriendCode, bool swapMods)
    {
        SenderFriendCode = senderFriendCode;
        SwapMods = swapMods;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapQueryRequest");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("SwapMods", SwapMods);
        return sb.ToString();
    }
}

public struct BodySwapQueryResponse
{
    public string? CharacterName { get; set; }
    public string? CharacterData { get; set; }
    
    public BodySwapQueryResponse(string? characterName, string? characterData)
    {
        CharacterName = characterName;
        CharacterData = characterData;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapQueryResponse");
        sb.AddVariable("CharacterName", CharacterName);
        sb.AddVariable("CharacterData", CharacterData);
        return sb.ToString();
    }
}
