namespace AetherRemoteCommon.Domain.Permissions.V2;

public class UserPermissionsV2
{
    public PrimaryPermissionsV2 Primary { get; set; } = PrimaryPermissionsV2.None;
    public LinkshellPermissionsV2 Linkshell { get; set; } = LinkshellPermissionsV2.None;

    public override string ToString()
    {
        var sb = new AetherRemoteStringBuilder("UserPermissionsV2");
        sb.AddVariable("PrimaryPermissionsV2", Primary.ToString());
        sb.AddVariable("LinkshellPermissionsV2", Linkshell.ToString());
        return sb.ToString();
    }
}