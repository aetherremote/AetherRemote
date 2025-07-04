using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteServer.Domain.Interfaces;

/// <summary>
///     TODO
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    ///     TODO
    /// </summary>
    public Task<string?> GetFriendCodeBySecret(string secret);

    /// <summary>
    ///     TODO
    /// </summary>
    public Task<DatabaseResultEc> CreatePermissions(string senderFriendCode, string targetFriendCode);

    /// <summary>
    ///     TODO
    /// </summary>
    public Task<DatabaseResultEc> UpdatePermissions(string senderFriendCode, string targetFriendCode, UserPermissions permissions);

    /// <summary>
    ///     TODO
    /// </summary>
    public Task<FriendPermissions> GetPermissions(string friendCode);

    /// <summary>
    ///     TODO
    /// </summary>
    public Task<DatabaseResultEc> DeletePermissions(string senderFriendCode, string targetFriendCode);
}