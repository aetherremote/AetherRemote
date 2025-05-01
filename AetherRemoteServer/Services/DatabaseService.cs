using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteServer.Domain;
using Microsoft.Data.Sqlite;

namespace AetherRemoteServer.Services;

/// <summary>
///     Provides methods for interacting with the underlying Sqlite3 database
/// </summary>
public class DatabaseService
{
    // Constants
    private const string ValidUsersTable = "ValidUsersTable";
    private const string PermissionsTable = "PermissionsTable";
    private const string SecretParam = "@Secret";
    private const string FriendCodeParam = "@FriendCode";
    private const string TargetFriendCodeParam = "@TargetFriendCode";
    private const string VersionParam = "@Version";
    private const string PrimaryPermissionsParam = "@PrimaryPermissions";
    private const string LinkshellPermissionsParam = "@LinkshellPermissions";
    private const int CurrentPermissionConfigurationVersion = 2;

    // Injected
    private readonly ILogger<DatabaseService> _logger;

    // Instantiated
    private readonly SqliteConnection _db;
    private readonly TypedMemoryCache<User> _userCache;
    private readonly TypedMemoryCache<FriendPermissions> _permissionsCache;

    /// <summary>
    ///     <inheritdoc cref="DatabaseService"/>
    /// </summary>
    public DatabaseService(ILogger<DatabaseService> logger)
    {
        // Inject
        _logger = logger;

        // Db file check
        var path = Path.Combine(Directory.GetCurrentDirectory(), "db", "main2.db");
        if (File.Exists(path) is false)
        {
            logger.LogInformation("Db directory {Directory} does not exist, creating!", path);
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "db"));
            File.WriteAllBytes(path, []);
        }

        // Open Db
        _db = new SqliteConnection($"Data Source={path}");
        _db.Open();
        _userCache = new TypedMemoryCache<User>();
        _permissionsCache = new TypedMemoryCache<FriendPermissions>();

        // Table validation
        InitializeDbTables();
    }

    /// <summary>
    ///     Gets a user entry from the valid users table by friend code
    /// </summary>
    public async Task<User?> GetUserByFriendCode(string friendCode)
    {
        if (_userCache.Get(friendCode) is { } cachedUser)
            return cachedUser;

        await using var command = _db.CreateCommand();
        command.CommandText = $"SELECT * FROM {ValidUsersTable} WHERE FriendCode = {FriendCodeParam}";
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync() is false)
                return null;

            var secret = reader.GetString(1);
            var isAdmin = reader.GetInt32(2) is 1;
            var user = new User(friendCode, secret, isAdmin);
            _userCache.Set(friendCode, user);

            return user;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to get user with friend code {FriendCode}, {Exception}", friendCode, e.Message);
            return null;
        }
    }

    /// <summary>
    ///     Gets a user entry from the valid users table by secret 
    /// </summary>
    public async Task<User?> GetUserBySecret(string secret)
    {
        await using var command = _db.CreateCommand();
        command.CommandText = $"SELECT * FROM {ValidUsersTable} WHERE Secret = {SecretParam}";
        command.Parameters.AddWithValue(SecretParam, secret);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync() is false)
                return null;

            var friendCode = reader.GetString(0);
            var isAdmin = reader.GetInt32(2) is 1;
            return new User(friendCode, secret, isAdmin);
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to get user with secret {Secret}, {Exception}", secret, e.Message);
            return null;
        }
    }

    /// <summary>
    ///     Creates an empty set of permissions between sender and target friend codes
    /// </summary>
    public async Task<bool> CreatePermissions(string senderFriendCode, string targetFriendCode)
    {
        await using var command = _db.CreateCommand();
        command.CommandText =
            $"""
                 INSERT INTO {PermissionsTable} (UserFriendCode, TargetFriendCode, Version, PrimaryPermissions, LinkshellPermissions)
                 VALUES ({FriendCodeParam}, {TargetFriendCodeParam}, {VersionParam}, {PrimaryPermissionsParam}, {LinkshellPermissionsParam})
             """;
        command.Parameters.AddWithValue(FriendCodeParam, senderFriendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriendCode);
        command.Parameters.AddWithValue(VersionParam, CurrentPermissionConfigurationVersion);
        command.Parameters.AddWithValue(PrimaryPermissionsParam, PrimaryPermissions.None);
        command.Parameters.AddWithValue(LinkshellPermissionsParam, LinkshellPermissions.None);

        try
        {
            return await command.ExecuteNonQueryAsync() is 1;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to create {FriendCode}'s permissions for {TargetFriendCode}, {Exception}",
                senderFriendCode, targetFriendCode, e.Message);
            return false;
        }
    }

    /// <summary>
    ///     Updates a set of permissions between sender and target friend codes
    /// </summary>
    public async Task<bool> UpdatePermissions(string senderFriendCode, string targetFriendCode,
        UserPermissions permissions)
    {
        await using var command = _db.CreateCommand();
        command.CommandText =
            $"""
                 UPDATE {PermissionsTable} 
                 SET PrimaryPermissions = {PrimaryPermissionsParam}, LinkshellPermissions = {LinkshellPermissionsParam} 
                 WHERE UserFriendCode = {FriendCodeParam} AND TargetFriendCode = {TargetFriendCodeParam}
             """;
        command.Parameters.AddWithValue(PrimaryPermissionsParam, permissions.Primary);
        command.Parameters.AddWithValue(LinkshellPermissionsParam, permissions.Linkshell);
        command.Parameters.AddWithValue(FriendCodeParam, senderFriendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriendCode);

        try
        {
            var success = await command.ExecuteNonQueryAsync() is 1;
            if (success)
                _permissionsCache.Remove(senderFriendCode);

            return success;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to update {FriendCode}'s permissions for {TargetFriendCode}, {Exception}",
                senderFriendCode, targetFriendCode, e.Message);
            return false;
        }
    }

    /// <summary>
    ///     Gets all the permissions sender has granted to others
    /// </summary>
    public async Task<FriendPermissions> GetPermissions(string friendCode)
    {
        if (_permissionsCache.Get(friendCode) is { } cachedPermissions)
            return cachedPermissions;

        await using var command = _db.CreateCommand();
        command.CommandText =
            $"""
                SELECT TargetFriendCode, Version, PrimaryPermissions, LinkshellPermissions 
                FROM {PermissionsTable} 
                WHERE UserFriendCode={FriendCodeParam}
             """;
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);

        var result = new Dictionary<string, UserPermissions>();
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var targetFriendCode = reader.GetString(0);
                var version = reader.GetInt32(1);
                var primary = reader.GetInt32(2);
                var linkshell = reader.GetInt32(3);

                var permissions = version switch
                {
                    _ => new UserPermissions
                        { Primary = (PrimaryPermissions)primary, Linkshell = (LinkshellPermissions)linkshell }
                };

                result.Add(targetFriendCode, permissions);
            }

            var friendPermissions = new FriendPermissions { Permissions = result };
            _permissionsCache.Set(friendCode, friendPermissions);
            return friendPermissions;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to get permissions for {FriendCode}, {Exception}", friendCode, e.Message);
            return new FriendPermissions();
        }
    }

    /// <summary>
    ///     Deletes a set of permissions between sender and target friend code
    /// </summary>
    public async Task<bool> DeletePermissions(string senderFriendCode, string targetFriendCode)
    {
        await using var command = _db.CreateCommand();
        command.CommandText =
            $"""
                DELETE FROM {PermissionsTable} 
                WHERE UserFriendCode={FriendCodeParam} AND TargetFriendCode={TargetFriendCodeParam}
             """;
        command.Parameters.AddWithValue(FriendCodeParam, senderFriendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriendCode);

        try
        {
            var rows = await command.ExecuteNonQueryAsync() is 1;
            if (rows)
                _permissionsCache.Remove(senderFriendCode);

            return rows;
        }
        catch (Exception e)
        {
            _logger.LogWarning("Unable to delete {FriendCode}'s permissions for {TargetFriendCode}, {Exception}",
                senderFriendCode, targetFriendCode, e);
            return false;
        }
    }

    private void InitializeDbTables()
    {
        using var initializerValidUsersTable = _db.CreateCommand();
        initializerValidUsersTable.CommandText =
            $"""
                 CREATE TABLE IF NOT EXISTS {ValidUsersTable} (
                     FriendCode TEXT PRIMARY KEY,
                     Secret TEXT NOT NULL UNIQUE,
                     IsAdmin INTEGER NOT NULL
                 )
             """;

        initializerValidUsersTable.ExecuteNonQuery();

        /*
         * This can be slightly confusing at first.
         *
         * The idea is that this table contains a mapping of
         * user A to user B as the primary key, and then the
         * permissions that user A is granting to user B as
         * an integer that is converted to UserPermissions.
         *
         * The absence of permissions from user A to user B
         * means that user A has not added user B.
         */
        using var initializePermissionsTable = _db.CreateCommand();
        initializePermissionsTable.CommandText =
            $"""
                 CREATE TABLE IF NOT EXISTS {PermissionsTable} (
                     UserFriendCode TEXT NOT NULL,
                     TargetFriendCode TEXT NOT NULL,
                     Version INTEGER NOT NULL,
                     PrimaryPermissions INTEGER NOT NULL,
                     LinkshellPermissions INTEGER NOT NULL,
                     PRIMARY KEY (UserFriendCode, TargetFriendCode),
                     FOREIGN KEY (UserFriendCode) REFERENCES {ValidUsersTable}(FriendCode),
                     FOREIGN KEY (TargetFriendCode) REFERENCES {ValidUsersTable}(FriendCode)
                 )
             """;

        initializePermissionsTable.ExecuteNonQuery();
    }
}