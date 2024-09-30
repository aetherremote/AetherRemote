namespace AetherRemoteCommon.Domain.Network;

public struct DeletePermissionsRequest
{
    public string TargetCode { get; set; }

    public DeletePermissionsRequest(string targetCode)
    {
        TargetCode = targetCode;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DeletePermissionsRequest");
        sb.AddVariable("TargetCode", TargetCode);
        return sb.ToString();
    }
}

public struct DeletePermissionsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }

    public DeletePermissionsResponse(bool success, string message = "")
    {
        Success = success;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("DeletePermissionsResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}
