using MessagePack;

namespace AetherRemoteCommon.Domain.Network.GetAccountData;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

[MessagePackObject(keyAsPropertyName: true)]
public record GetAccountDataResponse
{
    public GetAccountDataEc Result { get; set; }
    public string FriendCode { get; set; } = string.Empty;
    public Dictionary<string, UserPermissions> PermissionsGrantedToOthers { get; set; } = [];
    public Dictionary<string, UserPermissions> PermissionsGrantedByOthers { get; set; } = [];

    public GetAccountDataResponse()
    {
    }
    
    public GetAccountDataResponse(GetAccountDataEc result)
    {
        Result = result;
    }

    public GetAccountDataResponse(
        string friendCode,
        Dictionary<string, UserPermissions> permissionsGrantedToOthers,
        Dictionary<string, UserPermissions> permissionsGrantedByOthers)
    {
        Result = GetAccountDataEc.Success;
        FriendCode = friendCode;
        PermissionsGrantedToOthers = permissionsGrantedToOthers;
        PermissionsGrantedByOthers = permissionsGrantedByOthers;
    }
}