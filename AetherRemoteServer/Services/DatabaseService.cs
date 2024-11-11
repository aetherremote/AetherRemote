using AetherRemoteCommon.Domain;
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
    private const string PermissionsTable = "PermissionsTable";

    // Params
    private const string SecretParam = "@Secret";
    private const string FriendCodeParam = "@FriendCode";
    private const string TargetFriendCodeParam = "@TargetFriendCode";
    private const string PermissionsParam = "@Permissions";
    private const string IsAdminParam = "@IsAdmin";
    private const string IdentifierParam = "@Identifier";

    // Schema
    public const int PermissionVersion = 2;
    
    // Injected
    private readonly ILogger<DatabaseService> logger;

    // Instantiated
    private readonly SqliteConnection db;
    private readonly MemoryCache userCache;
    private readonly MemoryCache permissionsCache;

    /// <summary>
    /// <inheritdoc cref="DatabaseService"/>
    /// </summary>
    public DatabaseService(ServerConfiguration config, ILogger<DatabaseService> logger)
    {
        // Injected
        this.logger = logger;

        // Db file check
        var path = Path.Combine(Directory.GetCurrentDirectory(), "db", "main2.db");
        if (File.Exists(path) == false)
        {
            logger.LogInformation("Db directory {Directory} does not exist, creating!", path);
            Directory.CreateDirectory(Path.Combine(Directory.GetCurrentDirectory(), "db"));
            File.WriteAllBytes(path, []);
        }

        // Open Db
        db = new SqliteConnection($"Data Source={path}");
        db.Open();
        userCache = new MemoryCache(new MemoryCacheOptions());
        permissionsCache = new MemoryCache(new MemoryCacheOptions());

        // Table validation
        ValidateDbTables();

        // Add debug accounts
        _ = CreateOrUpdateUser("adminFriendCode", config.AdminAccountSecret, true);

        // _ = ConversionToV2.MigratePermissions(db, logger);
    }

    /// <summary>
    /// Creates or updates a user from the valid user table
    /// </summary>
    public async Task<int> CreateOrUpdateUser(string friendCode, string secret, bool isAdmin)
    {
        using var command = db.CreateCommand();
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
            if (rows > 0) userCache.Set(friendCode, new UserDb(friendCode, secret, isAdmin));
            return rows;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Unable to create or update data for friend code {FriendCode}, {Exception}", friendCode, ex);
            return 0;
        }
    }

    /// <summary>
    /// Deletes a user from the valid users table
    /// </summary>
    public async Task<int> DeleteUser(string friendCode)
    {
        using var command = db.CreateCommand();
        command.CommandText =
           $"""
                DELETE FROM {ValidUsersTable} WHERE FriendCode={FriendCodeParam}
            """;
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);

        try
        {
            // Update cache
            var rows = await command.ExecuteNonQueryAsync();
            if (rows > 0) userCache.Remove(friendCode);
            return rows;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Unable to delete user {FriendCode}, {Exception}", friendCode, ex);
            return 0;
        }
    }

    /// <summary>
    /// Retrieves a user from the user database. By default will search by friend code.
    /// </summary>
    public async Task<UserDb?> GetUser(string identifier, QueryUserType type = QueryUserType.FriendCode)
    {
        // Check if FriendCode identifier is cached
        if (type == QueryUserType.FriendCode && userCache.TryGetValue(identifier, out UserDb cachedUser))
            return cachedUser;

        using var command = db.CreateCommand();
        command.CommandText = $"SELECT * FROM {ValidUsersTable} WHERE {type}={IdentifierParam}";
        command.Parameters.AddWithValue(IdentifierParam, identifier);

        try
        {
            using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync() == false) return null;

            var friendCode = reader.GetString(0);
            var secret = reader.GetString(1);
            var isAdmin = reader.GetInt32(2) == 1;

            var user = new UserDb(friendCode, secret, isAdmin);
            if (type == QueryUserType.FriendCode) userCache.Set(identifier, user);
            return user;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Unable to retrieve UserDb by {Type}: {Identifier}, {Exception}", type, identifier, ex);
            return null;
        }
    }

    /// <summary>
    /// Creates or updates permissions for specified target
    /// </summary>
    public async Task<(int, string)> CreateOrUpdatePermissions(string friendCode, string targetFriendCode, UserPermissions permissions)
    {
        using var command = db.CreateCommand();
        command.CommandText =
           $"""
                INSERT OR REPLACE INTO {PermissionsTable} (UserFriendCode, TargetFriendCode, Permissions)
                    values ({FriendCodeParam}, {TargetFriendCodeParam}, {PermissionsParam})
            """;
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriendCode);
        command.Parameters.AddWithValue(PermissionsParam, (int)permissions);

        try
        {
            // Update cache
            var rows = await command.ExecuteNonQueryAsync();
            if (rows > 0) permissionsCache.Remove(friendCode); // Lazy implementation
            return (rows, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Unable to create or update permissions for {FriendCode}, {Exception}", friendCode, ex);
            return (0, ex.Message);
        }
    }

    /// <summary>
    /// Gets all permissions user has granted to others
    /// </summary>
    public async Task<Dictionary<string, UserPermissions>> GetPermissions(string friendCode)
    {
        if (permissionsCache.TryGetValue(friendCode, out Dictionary<string, UserPermissions>? cachedPermissions))
            return cachedPermissions ?? [];

        using var command = db.CreateCommand();
        command.CommandText = $"SELECT TargetFriendCode, Permissions FROM {PermissionsTable} WHERE UserFriendCode={FriendCodeParam}";
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);

        var result = new Dictionary<string, UserPermissions>();
        try
        {
            using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var targetFriendCode = reader.GetString(0);
                var permissions = reader.GetInt32(1);
                result.Add(targetFriendCode, (UserPermissions)permissions);
            }

            permissionsCache.Set(friendCode, result);
            return result;
        }
        catch (Exception ex)
        {
            logger.LogWarning("Unable to retrieve permission list for friend code {FriendCode}, {Exception}", friendCode, ex.Message);
            return [];
        }
    }

    /// <summary>
    /// Deletes the permissions a user has set for another 
    /// </summary>
    public async Task<(int, string)> DeletePermissions(string friendCode, string targetFriend)
    {
        using var command = db.CreateCommand();
        command.CommandText =
           $"""
                DELETE FROM {PermissionsTable} WHERE UserFriendCode={FriendCodeParam} AND TargetFriendCode={TargetFriendCodeParam}
            """;
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);
        command.Parameters.AddWithValue(TargetFriendCodeParam, targetFriend);

        try
        {
            var rows = await command.ExecuteNonQueryAsync();
            if (rows > 0) permissionsCache.Remove(friendCode);
            return (rows, string.Empty);
        }
        catch (Exception ex)
        {
            logger.LogWarning("Unable to delete permissions for friend code {FriendCode}, {Exception}", friendCode, ex);
            return (0, ex.Message);
        }
    }

    /// <summary>
    /// Clears all table data and cache data
    /// </summary>
    public void ClearTables()
    {
        // Must delete permissions table before valid users to preserve foreign key constraint
        using var permissionsDeleteCommand = db.CreateCommand();
        permissionsDeleteCommand.CommandText = $"DELETE FROM {PermissionsTable}";
        permissionsDeleteCommand.ExecuteNonQuery();

        using var validUserDeleteCommand = db.CreateCommand();
        validUserDeleteCommand.CommandText = $"DELETE FROM {ValidUsersTable}";
        validUserDeleteCommand.ExecuteNonQuery();

        userCache.Clear();
        permissionsCache.Clear();
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
        using var validationCommand = db.CreateCommand();
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
        using var friendshipCommand = db.CreateCommand();
        friendshipCommand.CommandText =
           $"""
                CREATE TABLE IF NOT EXISTS {PermissionsTable} (
                    UserFriendCode TEXT NOT NULL,
                    TargetFriendCode TEXT NOT NULL,
                    Permissions INTEGER NOT NULL,
                    PRIMARY KEY (UserFriendCode, TargetFriendCode),
                    FOREIGN KEY (UserFriendCode) REFERENCES {ValidUsersTable}(FriendCode),
                    FOREIGN KEY (TargetFriendCode) REFERENCES {ValidUsersTable}(FriendCode)
                )
            """;

        friendshipCommand.ExecuteNonQuery();
    }

    public void Dispose()
    {
        db.Close();
        db.Dispose();
        GC.SuppressFinalize(this);
    }
}
