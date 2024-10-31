namespace AetherRemoteCommon.Domain.Network.Commands;

public struct BodySwapRequest
{
    public List<string> TargetFriendCodes { get; set; }
    public bool SwapMods { get; set; }
    public string? CharacterName { get; set; }
    public string? CharacterData { get; set; }

    public BodySwapRequest(List<string> targetFriendCodes, bool swapMods, string? characterName, string? characterData)
    {
        TargetFriendCodes = targetFriendCodes;
        SwapMods = swapMods;
        CharacterName = characterName;
        CharacterData = characterData;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapRequest");
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("SwapMods", SwapMods);
        sb.AddVariable("CharacterName", CharacterName);
        sb.AddVariable("CharacterData", CharacterData);
        return sb.ToString();
    }
}

public struct BodySwapResponse
{
    public bool Success { get; set; }
    public string? CharacterName { get; set; }
    public string? CharacterData { get; set; }
    public string? Message { get; set; }

    public BodySwapResponse(
        bool success, 
        string? characterName = null, 
        string? characterData = null, 
        string ? message = null)
    {
        Success = success;
        CharacterName = characterName;
        CharacterData = characterData;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("CharacterName", CharacterName);
        sb.AddVariable("CharacterData", CharacterData);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

public struct BodySwapCommand
{
    public string SenderFriendCode { get; set; }
    public string? CharacterName { get; set; }
    public string CharacterData { get; set; }

    public BodySwapCommand(string senderFriendCode, string? characterName, string characterData)
    {
        SenderFriendCode = senderFriendCode;
        CharacterName = characterName;
        CharacterData = characterData;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapCommand");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("CharacterName", CharacterName);
        sb.AddVariable("CharacterData", CharacterData);
        return sb.ToString();
    }
}
