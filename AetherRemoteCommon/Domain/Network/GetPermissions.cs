namespace AetherRemoteCommon.Domain.Network;

public struct GetPermissionsRequest
{
    public GetPermissionsRequest() { }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("GetPermissionsRequest");
        return sb.ToString();
    }
}

public struct GetPermissionsResponse
{
    public bool Success { get; set; }
    public Dictionary<string, UserPermissions> Permissions { get; set; }
    public string Message { get; set; }

    public GetPermissionsResponse(bool success, Dictionary<string, UserPermissions> permissions, string message = "")
    {
        Success = success;
        Permissions = permissions;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("GetPermissionsResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Permissions", Permissions);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
