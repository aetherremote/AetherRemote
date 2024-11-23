using AetherRemoteCommon.Domain.Permissions;

namespace AetherRemoteCommon.Domain.Network;

public struct LoginDetailsRequest
{
    public LoginDetailsRequest() { }

    public readonly override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginDetailsRequest");
        return sb.ToString();
    }
}

public struct LoginDetailsResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string? FriendCode { get; set; }
    public Dictionary<string, UserPermissions>? PermissionsGrantedToOthers { get; set; }
    public Dictionary<string, UserPermissions>? PermissionsGrantedByOthers { get; set; }

    public LoginDetailsResponse(
        bool success, string? 
        friendCode = null, 
        Dictionary<string, UserPermissions>? permissionsGrantedToOthers = null,
        Dictionary<string, UserPermissions>? permissionsGrantedByOthers = null,
        string message = "")
    {
        Success = success;
        Message = message;
        FriendCode = friendCode;
        PermissionsGrantedToOthers = permissionsGrantedToOthers;
        PermissionsGrantedByOthers = permissionsGrantedByOthers;
    }

    public readonly override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("LoginDetailsResponse");
        sb.AddVariable("Success", Success);
        sb.AddVariable("Message", Message);
        sb.AddVariable("FriendCode", FriendCode);
        sb.AddVariable("PermissionsGrantedToOthers", PermissionsGrantedToOthers);
        sb.AddVariable("PermissionsGrantedByOthers", PermissionsGrantedByOthers);
        return sb.ToString();
    }
}
