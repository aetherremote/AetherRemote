using System.Text.Json;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Permissions.V2;
using AetherRemoteServer.Services;
using Microsoft.Data.Sqlite;

namespace AetherRemoteServer.Domain;

public static class ConversionToV2
{
    private const string ValidUsersTable = "ValidUsersTable";
    private const string PermissionsTable = "PermissionsTable";
    private const string PermissionsTableV2 = "PermissionsTableV2";
    
    private const string FriendCodeParam = "@FriendCode";
    private const string TargetFriendCodeParam = "@TargetFriendCode";
    
    private const string PermissionsParam = "@Permissions";
    private const string VersionParam = "@Version";
    public static async Task MigratePermissions(SqliteConnection db, ILogger<DatabaseService> logger)
    {
        await using var friendshipCommandV2 = db.CreateCommand();
        friendshipCommandV2.CommandText =
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

        friendshipCommandV2.ExecuteNonQuery();

        await using var command = db.CreateCommand();
        command.CommandText = $"SELECT * FROM {PermissionsTable}";
        try
        {
            await using var reader = await command.ExecuteReaderAsync();
            while (reader.Read())
            {
                var user = reader.GetString(0);
                var target = reader.GetString(1);
                var permissions = reader.GetInt32(2);

                var userPermissions = (UserPermissions)permissions;

                var primaryPermissions = PrimaryPermissionsV2.None;
                var linkshellPermissions = LinkshellPermissionsV2.None;
                
                // Conversion
                if (userPermissions.HasFlag(UserPermissions.Speak)) primaryPermissions |= PrimaryPermissionsV2.Speak;
                if (userPermissions.HasFlag(UserPermissions.Emote)) primaryPermissions |= PrimaryPermissionsV2.Speak;
                
                if (userPermissions.HasFlag(UserPermissions.Customization)) primaryPermissions |= PrimaryPermissionsV2.Customization;
                if (userPermissions.HasFlag(UserPermissions.Equipment)) primaryPermissions |= PrimaryPermissionsV2.Equipment;
                
                if (userPermissions.HasFlag(UserPermissions.Say)) primaryPermissions |= PrimaryPermissionsV2.Say;
                if (userPermissions.HasFlag(UserPermissions.Yell)) primaryPermissions |= PrimaryPermissionsV2.Yell;
                if (userPermissions.HasFlag(UserPermissions.Shout)) primaryPermissions |= PrimaryPermissionsV2.Shout;
                if (userPermissions.HasFlag(UserPermissions.Tell)) primaryPermissions |= PrimaryPermissionsV2.Tell;
                if (userPermissions.HasFlag(UserPermissions.Party)) primaryPermissions |= PrimaryPermissionsV2.Party;
                if (userPermissions.HasFlag(UserPermissions.Alliance)) primaryPermissions |= PrimaryPermissionsV2.Alliance;
                if (userPermissions.HasFlag(UserPermissions.FreeCompany)) primaryPermissions |= PrimaryPermissionsV2.FreeCompany;
                if (userPermissions.HasFlag(UserPermissions.PvPTeam)) primaryPermissions |= PrimaryPermissionsV2.PvPTeam;

                if (userPermissions.HasFlag(UserPermissions.LS1)) linkshellPermissions |= LinkshellPermissionsV2.Ls1;
                if (userPermissions.HasFlag(UserPermissions.LS2)) linkshellPermissions |= LinkshellPermissionsV2.Ls2;
                if (userPermissions.HasFlag(UserPermissions.LS3)) linkshellPermissions |= LinkshellPermissionsV2.Ls3;
                if (userPermissions.HasFlag(UserPermissions.LS4)) linkshellPermissions |= LinkshellPermissionsV2.Ls4;
                if (userPermissions.HasFlag(UserPermissions.LS5)) linkshellPermissions |= LinkshellPermissionsV2.Ls5;
                if (userPermissions.HasFlag(UserPermissions.LS6)) linkshellPermissions |= LinkshellPermissionsV2.Ls6;
                if (userPermissions.HasFlag(UserPermissions.LS7)) linkshellPermissions |= LinkshellPermissionsV2.Ls7;
                if (userPermissions.HasFlag(UserPermissions.LS8)) linkshellPermissions |= LinkshellPermissionsV2.Ls8;
                
                if (userPermissions.HasFlag(UserPermissions.CWL1)) linkshellPermissions |= LinkshellPermissionsV2.Cwl1;
                if (userPermissions.HasFlag(UserPermissions.CWL2)) linkshellPermissions |= LinkshellPermissionsV2.Cwl2;
                if (userPermissions.HasFlag(UserPermissions.CWL3)) linkshellPermissions |= LinkshellPermissionsV2.Cwl3;
                if (userPermissions.HasFlag(UserPermissions.CWL4)) linkshellPermissions |= LinkshellPermissionsV2.Cwl4;
                if (userPermissions.HasFlag(UserPermissions.CWL5)) linkshellPermissions |= LinkshellPermissionsV2.Cwl5;
                if (userPermissions.HasFlag(UserPermissions.CWL6)) linkshellPermissions |= LinkshellPermissionsV2.Cwl6;
                if (userPermissions.HasFlag(UserPermissions.CWL7)) linkshellPermissions |= LinkshellPermissionsV2.Cwl7;
                if (userPermissions.HasFlag(UserPermissions.CWL8)) linkshellPermissions |= LinkshellPermissionsV2.Cwl8;

                if (userPermissions.HasFlag(UserPermissions.ModSwap)) primaryPermissions |= PrimaryPermissionsV2.Mods;
                
                logger.LogInformation("Converting {OldPerms} to {NewPerms} and {NewerPerms}", userPermissions, primaryPermissions, linkshellPermissions);

                var final = new PermissionsV2(primaryPermissions, linkshellPermissions);
                var text = JsonSerializer.Serialize(final);
                
                await using var insert = db.CreateCommand();
                insert.CommandText =
                    $"""
                         INSERT INTO {PermissionsTableV2} (UserFriendCode, TargetFriendCode, Version, Permissions) values 
                         ({FriendCodeParam}, {TargetFriendCodeParam}, {VersionParam}, {PermissionsParam})
                     """;
                insert.Parameters.AddWithValue(FriendCodeParam, user);
                insert.Parameters.AddWithValue(TargetFriendCodeParam, target);
                insert.Parameters.AddWithValue(VersionParam, DatabaseService.PermissionVersion);
                insert.Parameters.AddWithValue(PermissionsParam, text);
                
                await insert.ExecuteNonQueryAsync();
                logger.LogInformation("Successfully updated");
            }
        }
        catch (Exception ex)
        {
            logger.LogWarning("Something went wrong: {Exception}", ex);
        }
    }
}