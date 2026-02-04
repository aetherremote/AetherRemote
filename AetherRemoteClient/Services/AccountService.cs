using System;
using System.Threading.Tasks;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides information about the account currently connected in an online session
/// </summary>
public class AccountService
{
    public string FriendCode { get; private set; } = "Unknown Friend Code";

    public ResolvedPermissions GlobalPermissions { get; private set; } = new(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.None);

    /// <summary>
    ///     Event fired when the global permissions are updated
    /// </summary>
    public event Func<ResolvedPermissions, Task>? GlobalPermissionsUpdated;
    
    public void SetFriendCode(string friendCode) => FriendCode = friendCode;

    public void SetGlobalPermissions(ResolvedPermissions globalPermissions)
    {
        GlobalPermissions = globalPermissions;
        GlobalPermissionsUpdated?.Invoke(GlobalPermissions);
    }
}