using AetherRemoteCommon.Domain.Permissions;

namespace AetherRemoteCommon.Domain.Network;

public struct CreateOrUpdatePermissionsRequest
{
    public string TargetCode { get; set; }
    public UserPermissions Permissions { get; set; }

    public CreateOrUpdatePermissionsRequest(string targetCode, UserPermissions permissions)
    {
        TargetCode = targetCode;
        Permissions = permissions;
    }

    public readonly override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("CreateOrUpdatePermissionsRequest");
        sb.AddVariable("TargetCode", TargetCode);
        sb.AddVariable("UserPermissions", Permissions);
        return sb.ToString();
    }
}

public struct CreateOrUpdatePermissionsResponse
{
    public bool Success { get; set; }
    public bool Online { get; set; }
    public string Message { get; set; }

    public CreateOrUpdatePermissionsResponse(bool success, bool online, string message = "")
    {
        Success = success;
        Online = online;
        Message = message;
    }

    public override readonly string ToString()
    {
        var sb = new AetherRemoteStringBuilder("CreateOrUpdatePermissionsResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Online", Online);
        sb.AddVariable("Message", Message);
        return sb.ToString();
    }
}

