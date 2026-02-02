using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteServer.Domain.Shared;

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
    public Task<DatabaseResultEc> UpdatePermissions(string senderFriendCode, string targetFriendCode, RawPermissions permissions);

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

    /// <summary>
    ///     Updates a user's global permissions
    /// </summary>
    public Task<DatabaseResultEc> UpdateGlobalPermissions(string senderFriendCode, ResolvedPermissions permissions);
    
    /// <summary>
    ///     Gets a user's global permissions
    /// </summary>
    public Task<ResolvedPermissions?> GetGlobalPermissions(string senderFriendCode);
    
    /// <summary>
    ///     Admin function to add a new account to the database
    /// </summary>
    /// <returns></returns>
    public Task<DatabaseResultEc> AdminCreateAccount(ulong discord, string friendCode, string secret);

    /// <summary>
    ///     Attempts to get all the friend codes associated with provided discord id
    /// </summary>
    /// <param name="discord">Discord Id</param>
    public Task<List<Account>?> AdminGetAccounts(ulong discord);

    /// <summary>
    ///     Attempts to update an existing account's friend code, and all the relationships associated with them
    /// </summary>
    public Task<DatabaseResultEc> AdminUpdateAccount(ulong discord, string oldFriendCode, string newFriendCode);

    /// <summary>
    ///     Attempts to delete an existing account, and all associated relationships.
    /// </summary>
    public Task<DatabaseResultEc> AdminDeleteAccount(ulong discord, string friendCode);
}