namespace AetherRemoteCommon.Domain.Network.Commands;

public struct BodySwapRequest
{
    public List<string> TargetFriendCodes { get; set; }
    public string? CharacterData { get; set; }

    public BodySwapRequest(List<string> targetFriendCodes, string? characterData)
    {
        TargetFriendCodes = targetFriendCodes;
        CharacterData = characterData;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapRequest");
        sb.AddVariable("TargetFriendCodes", TargetFriendCodes);
        sb.AddVariable("CharacterData", CharacterData);
        return sb.ToString();
    }
}

public struct BodySwapResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public string? CharacterData { get; set; }

    public BodySwapResponse(bool success, string? message = null, string? characterData = null)
    {
        Success = success;
        Message = message;
        CharacterData = characterData;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("CharacterData", CharacterData);
        return sb.ToString();
    }
}

public struct BodySwapCommand
{
    public string SenderFriendCode { get; set; }
    public string CharacterData { get; set; }

    public BodySwapCommand(string senderFriendCode, string characterData)
    {
        SenderFriendCode = senderFriendCode;
        CharacterData = characterData;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("BodySwapCommand");
        sb.AddVariable("SenderFriendCode", SenderFriendCode);
        sb.AddVariable("CharacterData", CharacterData);
        return sb.ToString();
    }
}
