using AetherRemoteCommon.Domain;

namespace AetherRemoteServer.Domain.Interfaces;

/// <summary>
///     TODO
/// </summary>
public interface IDatabaseService
{
    /// <summary>
    ///     TODO
    /// </summary>
    public Task<User?> GetUserByFriendCode(string friendCode);

    /// <summary>
    ///     TODO
    /// </summary>
    public Task<User?> GetUserBySecret(string secret);

    /// <summary>
    ///     TODO
    /// </summary>
    public Task<bool> CreatePermissions(string senderFriendCode, string targetFriendCode);

    /// <summary>
    ///     TODO
    /// </summary>
    public Task<bool> UpdatePermissions(string senderFriendCode, string targetFriendCode, UserPermissions permissions);

    /// <summary>
    ///     TODO
    /// </summary>
    public Task<FriendPermissions> GetPermissions(string friendCode);

    /// <summary>
    ///     TODO
    /// </summary>
    public Task<bool> DeletePermissions(string senderFriendCode, string targetFriendCode);
}