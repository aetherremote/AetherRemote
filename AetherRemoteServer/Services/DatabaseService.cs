using System.Text.Json;
using AetherRemoteCommon.Domain.Permissions.V2;
using AetherRemoteServer.Domain;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;

namespace AetherRemoteServer.Services;

/// <summary>
/// Provides methods to interact with and query the database
/// </summary>
public class DatabaseService : IDisposable
{
    // Tables
    private const string ValidUsersTable = "ValidUsersTable";
   // private const string PermissionsTable = "PermissionsTable";
    private const string PermissionsTableV2 = "PermissionsTableV2";

    // Params
    private const string SecretParam = "@Secret";
    private const string FriendCodeParam = "@FriendCode";
    private const string TargetFriendCodeParam = "@TargetFriendCode";
    private const string PermissionsParam = "@Permissions";
    private const string IsAdminParam = "@IsAdmin";
    private const string IdentifierParam = "@Identifier";
    private const string VersionParam = "@Version";

    // Schema
    public const int CurrentPermissionConfigurationVersion = 2;
    
    // Injected
    private readonly ILogger<DatabaseService> _logger;

    // Instantiated
    private readonly SqliteConnection _db;
    private readonly MemoryCache _userCache;
    private readonly MemoryCache _permissionsCache;

    /// <summary>
    /// <inheritdoc cref="DatabaseService"/>
    /// </summary>
    public DatabaseService(ServerConfiguration config, ILogger<DatabaseService> logger)
    {
        // Injected
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
        _userCache = new MemoryCache(new MemoryCacheOptions());
        _permissionsCache = new MemoryCache(new MemoryCacheOptions());

        // Table validation
        ValidateDbTables();

        // Add debug accounts
        _ = CreateOrUpdateUser("adminFriendCode", config.AdminAccountSecret, true);
    }

    /// <summary>
    /// Creates or updates a user from the valid user table
    /// </summary>
    public async Task<int> CreateOrUpdateUser(string friendCode, string secret, bool isAdmin)
    {
        await using var command = _db.CreateCommand();
        command.CommandText =
           $"""
                INSERT OR REPLACE INTO {ValidUsersTable} (FriendCode, Secret, IsAdmin)
                    values ({FriendCodeParam}, {SecretParam}, {IsAdminParam})
            """;
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);
        command.Parameters.AddWithValue(SecretParam, secret);
        command.Parameters.AddWithValue(IsAdminParam, isAdmin ? 1 : 0);

        try
        {
            // Update cache
            var rows = await command.ExecuteNonQueryAsync();
            if (rows > 0) _userCache.Set(friendCode, new UserDb(friendCode, secret, isAdmin));
            return rows;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unable to create or update data for friend code {FriendCode}, {Exception}", friendCode, ex);
            return 0;
        }
    }

    /// <summary>
    /// Deletes a user from the valid users table
    /// </summary>
    public async Task<int> DeleteUser(string friendCode)
    {
        await using var command = _db.CreateCommand();
        command.CommandText = $"DELETE FROM {ValidUsersTable} WHERE FriendCode={FriendCodeParam}";
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);

        try
        {
            // Update cache
            var rows = await command.ExecuteNonQueryAsync();
            if (rows > 0) _userCache.Remove(friendCode);
            return rows;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unable to delete user {FriendCode}, {Exception}", friendCode, ex);
            return 0;
        }
    }

    /// <summary>
    /// Retrieves a user from the user database. By default, will search by friend code.
    /// </summary>
    public async Task<UserDb?> GetUser(string identifier, QueryUserType type = QueryUserType.FriendCode)
    {
        // Check if FriendCode identifier is cached
        if (type == QueryUserType.FriendCode && _userCache.TryGetValue(identifier, out UserDb cachedUser))
            return cachedUser;

        await using var command = _db.CreateCommand();
        command.CommandText = $"SELECT * FROM {ValidUsersTable} WHERE {type}={IdentifierParam}";
        command.Parameters.AddWithValue(IdentifierParam, identifier);

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync() == false) return null;

            var friendCode = reader.GetString(0);
            var secret = reader.GetString(1);
            var isAdmin = reader.GetInt32(2) == 1;

            var user = new UserDb(friendCode, secret, isAdmin);
            if (type == QueryUserType.FriendCode) _userCache.Set(identifier, user);
            return user;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unable to retrieve UserDb by {Type}: {Identifier}, {Exception}", type, identifier, ex);
            return null;
        }
    }

    /// <summary>
    /// Creates or updates permissions for specified target
    /// </summary>
    public async Task<(int, string)> CreateOrUpdatePermissions(string friendCode, string targetFriendCode, UserPermissionsV2 permissions)
    {
        var deserializedPermissions = JsonSerializer.Serialize(permissions);
        
        await using var command = _db.CreateCommand();
        command.CommandText =
           $"""
                INSERT OR REPLACE INTO {PermissionsTableV2} (UserFriendCode, TargetFriendCode, Version, Permissions)
                    values ({FriendCodeParam}, {TargetFriendCodeParam}, {VersionParam}, {PermissionsParam})
            """;
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriendCode);
        command.Parameters.AddWithValue(VersionParam, CurrentPermissionConfigurationVersion);
        command.Parameters.AddWithValue(PermissionsParam, deserializedPermissions);

        try
        {
            // Update cache
            var rows = await command.ExecuteNonQueryAsync();
            if (rows > 0) _permissionsCache.Remove(friendCode); // Lazy implementation
            return (rows, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unable to create or update permissions for {FriendCode}, {Exception}", friendCode, ex);
            return (0, ex.Message);
        }
    }

    /// <summary>
    /// Gets all permissions user has granted to others
    /// </summary>
    public async Task<Dictionary<string, UserPermissionsV2>> GetPermissions(string friendCode)
    {
        if (_permissionsCache.TryGetValue(friendCode, out Dictionary<string, UserPermissionsV2>? cachedPermissions))
            return cachedPermissions ?? [];

        await using var command = _db.CreateCommand();
        command.CommandText = $"SELECT TargetFriendCode, Version, Permissions FROM {PermissionsTableV2} WHERE UserFriendCode={FriendCodeParam}";
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);

        var result = new Dictionary<string, UserPermissionsV2>();
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var targetFriendCode = reader.GetString(0);
                var version = reader.GetInt32(1);
                var json = reader.GetString(2);

                // If there are more than one version, make sure proper upgrading occurs
                var permissions = version switch
                {
                    _ => JsonSerializer.Deserialize<UserPermissionsV2>(json),
                };
                
                if (permissions is null)
                {
                    _logger.LogWarning("Json could not be parsed for {FriendCode}'s permissions for {TargetFriendCode}!", friendCode, targetFriendCode);
                    permissions = new UserPermissionsV2();
                }
                
                result.Add(targetFriendCode, permissions);
            }

            _permissionsCache.Set(friendCode, result);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unable to retrieve permission list for friend code {FriendCode}, {Exception}", friendCode, ex.Message);
            return [];
        }
    }

    /// <summary>
    /// Deletes the permissions a user has set for another 
    /// </summary>
    public async Task<(int, string)> DeletePermissions(string friendCode, string targetFriend)
    {
        await using var command = _db.CreateCommand();
        command.CommandText = $"DELETE FROM {PermissionsTableV2} WHERE UserFriendCode={FriendCodeParam} AND TargetFriendCode={TargetFriendCodeParam}";
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriend);

        try
        {
            var rows = await command.ExecuteNonQueryAsync();
            if (rows > 0) _permissionsCache.Remove(friendCode);
            return (rows, string.Empty);
        }
        catch (Exception ex)
        {
            _logger.LogWarning("Unable to delete permissions for friend code {FriendCode}, {Exception}", friendCode, ex);
            return (0, ex.Message);
        }
    }

    /// <summary>
    /// Clears all table data and cache data
    /// </summary>
    public void ClearTables()
    {
        // Must delete permissions table before valid users to preserve foreign key constraint
        using var permissionsDeleteCommand = _db.CreateCommand();
        permissionsDeleteCommand.CommandText = $"DELETE FROM {PermissionsTableV2}";
        permissionsDeleteCommand.ExecuteNonQuery();

        using var validUserDeleteCommand = _db.CreateCommand();
        validUserDeleteCommand.CommandText = $"DELETE FROM {ValidUsersTable}";
        validUserDeleteCommand.ExecuteNonQuery();

        _userCache.Clear();
        _permissionsCache.Clear();
    }

    /// <summary>
    /// How should <see cref="GetUser"/> search for a user?
    /// </summary>
    [Flags]
    public enum QueryUserType
    {
        FriendCode = 1 << 0,
        Secret = 1 << 1
    }

    /// <summary>
    /// Makes sure all tables exist, and if they don't create them
    /// </summary>
    private void ValidateDbTables()
    {
        using var validationCommand = _db.CreateCommand();
        validationCommand.CommandText =
           $"""
                CREATE TABLE IF NOT EXISTS {ValidUsersTable} (
                    FriendCode TEXT PRIMARY KEY,
                    Secret TEXT NOT NULL UNIQUE,
                    IsAdmin INTEGER NOT NULL
                )
            """;

        validationCommand.ExecuteNonQuery();

        /*
         * This can be slightly confusing at first
         * 
         * The idea is that this table contains a mapping of
         * user A to user B as the primary key, and then the
         * permissions that user A is granting to user B as
         * an integer that is converted to UserPermissions.
         * 
         * The absence of permissions from user A to user B
         * means that user A has not added user B.
         */
        using var friendshipCommand = _db.CreateCommand();
        friendshipCommand.CommandText =
            $"""
                 CREATE TABLE IF NOT EXISTS {PermissionsTableV2} (
                     UserFriendCode TEXT NOT NULL,
                     TargetFriendCode TEXT NOT NULL,
                     Version INTEGER NOT NULL,
                     Permissions TEXT NOT NULL,
                     PRIMARY KEY (UserFriendCode, TargetFriendCode),
                     FOREIGN KEY (UserFriendCode) REFERENCES {ValidUsersTable}(FriendCode),
                     FOREIGN KEY (TargetFriendCode) REFERENCES {ValidUsersTable}(FriendCode)
                 )
             """;

        friendshipCommand.ExecuteNonQuery();
    }

    public void Dispose()
    {
        _db.Close();
        _db.Dispose();
        GC.SuppressFinalize(this);
    }
}
