namespace AetherRemoteCommon.Domain.Permissions.V2;

public class PermissionsV2(PrimaryPermissionsV2 primary, LinkshellPermissionsV2 linkshell)
{
    public PrimaryPermissionsV2 Primary { get; set; } = primary;
    public LinkshellPermissionsV2 Linkshell { get; set; } = linkshell;
}