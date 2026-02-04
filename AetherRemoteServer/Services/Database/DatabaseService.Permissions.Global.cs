using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteServer.Services.Database;

public partial class DatabaseService
{
    /// <summary>
    ///     Gets a friend code's global permissions
    /// </summary>
    public async Task<ResolvedPermissions> GetGlobalPermissions(string friendCode)
    {
        await using var command = _database.CreateCommand();
        command.Parameters.AddWithValue("@friendCode", friendCode);
        command.CommandText = 
            """
                SELECT PrimaryPermissions, SpeakPermissions, ElevatedPermissions
                FROM GlobalPermissions
                WHERE FriendCode = @friendCode LIMIT 1
            """;

        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync() is false)
                return new ResolvedPermissions(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.None);
            
            var primary = (PrimaryPermissions2)reader.GetInt32(0);
            var speak = (SpeakPermissions2)reader.GetInt32(1);
            var elevated = (ElevatedPermissions)reader.GetInt32(2);
            return new ResolvedPermissions(primary, speak, elevated);
        }
        catch (Exception e)
        {
            _logger.LogError("[GetGlobalPermissions] {Error}", e);
            return new ResolvedPermissions(PrimaryPermissions2.None, SpeakPermissions2.None, ElevatedPermissions.None);
        }
    }
    
    /// <summary>
    ///     Updates a friend code's global permissions
    /// </summary>
    public async Task<DatabaseResultEc> UpdateGlobalPermissions(string friendCode, ResolvedPermissions permissions)
    {
        await using var command = _database.CreateCommand();
        command.CommandText = 
            """
                INSERT INTO GlobalPermissions (FriendCode, PrimaryPermissions, SpeakPermissions, ElevatedPermissions)
                VALUES (@friendCode, @primary, @speak, @elevated)
                ON CONFLICT(FriendCode)
                DO UPDATE SET PrimaryPermissions = excluded.PrimaryPermissions, SpeakPermissions = excluded.SpeakPermissions, ElevatedPermissions = excluded.ElevatedPermissions;
            """;
        
        command.Parameters.AddWithValue("@friendCode", friendCode);
        command.Parameters.AddWithValue("@primary", permissions.Primary);
        command.Parameters.AddWithValue("@speak", permissions.Speak);
        command.Parameters.AddWithValue("@elevated", permissions.Elevated);

        try
        {
            return await command.ExecuteNonQueryAsync() is 1 ? DatabaseResultEc.Success : DatabaseResultEc.NoOp;
        }
        catch (Exception e)
        {
            _logger.LogWarning("[UpdateGlobalPermissions] {Error}", e);
            return DatabaseResultEc.Unknown;
        }
    }
}