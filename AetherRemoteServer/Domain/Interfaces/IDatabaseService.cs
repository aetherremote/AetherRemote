using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;

namespace AetherRemoteServer.Domain.Interfaces;

/// <summary>
///     Provides access to the underlying Sqlite3 database
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    ///     Retrieves a friend code by secret, for use when first connecting to the server
    /// </summary>
    public Task<string?> GetFriendCodeBySecret(string secret);

    /// <summary>
    ///     Creates a new blank permission set between two users
    /// </summary>
    public Task<DatabaseResultEc> CreatePermissions(string senderFriendCode, string targetFriendCode);

    /// <summary>
    ///     Updates a permission set between two users
    /// </summary>
    public Task<DatabaseResultEc> UpdatePermissions(string senderFriendCode, string targetFriendCode, UserPermissions permissions);

    /// <summary>
    ///     Returns the permissions a friend has granted another, if they exists
    /// </summary>
    public Task<UserPermissions?> GetPermissions(string friendCode, string targetFriendCode);
    
    /// <summary>
    ///     Returns a list of all the permissions that friend code has granted others, as well as the permissions those friends have granted the friend code
    /// </summary>
    public Task<List<TwoWayPermissions>> GetAllPermissions(string friendCode);

    /// <summary>
    ///     Deletes a permission set between two users
    /// </summary>
    public Task<DatabaseResultEc> DeletePermissions(string senderFriendCode, string targetFriendCode);
}