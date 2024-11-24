namespace AetherRemoteCommon.Domain.Network.Commands;

public struct TwinningRequest
{
    public List<string> TargetFriendCodes { get; set; }
    public string CharacterData { get; set; }
    public string? CharacterName { get; set; }

    public TwinningRequest(List<string> targetFriendCodes, string characterData, string? characterName)
    {
        TargetFriendCodes = targetFriendCodes;
        CharacterData = characterData;
        CharacterName = characterName;
    }

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("TwinningRequest");
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("CharacterData", CharacterData);
        sb.AddVariable("CharacterName", CharacterName);
        return sb.ToString();
    }
}

public struct TwinningResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public TwinningResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public readonly override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("TwinningResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

public struct TwinningCommand
{
    public string SenderFriendCode { get; set; }
    public string CharacterData { get; set; }
    public string? CharacterName { get; set; }

    public TwinningCommand(string senderFriendCode, string characterData, string? characterName)
    {
        SenderFriendCode = senderFriendCode;
        CharacterData = characterData;
        CharacterName = characterName;
    }
    
    public readonly override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("TwinningCommand");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("CharacterData", CharacterData);
        sb.AddVariable("CharacterName", CharacterName);
        return sb.ToString();
    }
}