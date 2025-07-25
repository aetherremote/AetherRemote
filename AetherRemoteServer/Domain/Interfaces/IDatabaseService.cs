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
    ///     Retrieves all the permission sets for a specific user
    /// </summary>
    public Task<FriendPermissions> GetPermissions(string friendCode);

    /// <summary>
    ///     Deletes a permission set between two users
    /// </summary>
    public Task<DatabaseResultEc> DeletePermissions(string senderFriendCode, string targetFriendCode);
}