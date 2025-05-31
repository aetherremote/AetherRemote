using MessagePack;

namespace AetherRemoteCommon.Domain.Network;

// ReSharper disable PropertyCanBeMadeInitOnly.Global

[MessagePackObject(keyAsPropertyName: true)]
public record GetAccountDataResponse : BaseResponse
{
    public string FriendCode { get; set; } = string.Empty;
    public Dictionary<string, UserPermissions> PermissionsGrantedToOthers { get; set; } = [];
    public Dictionary<string, UserPermissions> PermissionsGrantedByOthers { get; set; } = [];
}