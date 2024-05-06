using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteServer.Domain;
using Microsoft.Data.Sqlite;
using System.Text.Json;

namespace AetherRemoteServer.Services;

public class DatabaseProvider : IDisposable
{
    private const string TableName = "Database";
    private static readonly string TablePath = Path.Combine("Data", TableName);
    private static readonly string ConnectionConfiguration = $"Data Source={TablePath}.db";
    private static readonly string MakeTableCommandText = $"CREATE TABLE {TableName} (Secret TEXT PRIMARY KEY, FriendCode TEXT, FriendList TEXT)";

    private const string SecretParam = @"$secret";
    private const string FriendCodeParam = @"$friendCode";
    private const string FriendListParam = @"$friendList";

    private readonly SqliteConnection db;

    public DatabaseProvider()
    {
        db = new SqliteConnection(ConnectionConfiguration);
        db.Open();

        // DumpTable();

        // MakeTable();
    }

    private void DumpTable()
    {
        var command = db.CreateCommand();
        command.CommandText = $"SELECT * FROM {TableName}";

        using var reader = command.ExecuteReader();
        while (reader.Read())
        {
            var _secret = reader.GetString(0);
            var _friendCode = reader.GetString(1);

            List<Friend> _friendList = [];
            if (reader.IsDBNull(2) == false)
            {
                _friendList = JsonSerializer.Deserialize<List<Friend>>(reader.GetString(2)) ?? [];
            }

            Console.WriteLine($"Secret: {_secret} FriendCode: {_friendCode} FriendList: {string.Join(",", _friendList)}");
        }
    }

    private void MakeTable()
    {
        var command = db.CreateCommand();
        command.CommandText = MakeTableCommandText;
        command.ExecuteNonQuery();
    }

    public void CreateOrUpdateUserData(UserData userData)
    {
        var serializedFriendList = JsonSerializer.Serialize(userData.FriendList);
        var command = db.CreateCommand();
        command.CommandText = $"INSERT OR REPLACE INTO {TableName} (Secret, FriendCode, FriendList) values ({SecretParam}, {FriendCodeParam}, {FriendListParam})";
        command.Parameters.AddWithValue(SecretParam, userData.Secret);
        command.Parameters.AddWithValue(FriendCodeParam, userData.FriendCode);
        command.Parameters.AddWithValue(FriendListParam, serializedFriendList);
        command.ExecuteNonQuery();
    }

    public UserData? TryGetUserDataBySecret(string secret)
    {
        var command = db.CreateCommand();
        command.CommandText = $"SELECT * FROM {TableName} WHERE Secret = {SecretParam}";
        command.Parameters.AddWithValue(SecretParam, secret);

        return TryGetUserData(command);
    }

    public UserData? TryGetUserDataByFriendCode(string friendCode)
    {
        var command = db.CreateCommand();
        command.CommandText = $"SELECT * FROM {TableName} WHERE FriendCode = {FriendCodeParam}";
        command.Parameters.AddWithValue(FriendCodeParam, friendCode);

        return TryGetUserData(command);
    }

    private static UserData? TryGetUserData(SqliteCommand command)
    {
        UserData? userData = null;
        try
        {
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var _secret = reader.GetString(0);
                var _friendCode = reader.GetString(1);

                List<Friend> _friendList = [];
                if (reader.IsDBNull(2) == false)
                {
                    _friendList = JsonSerializer.Deserialize<List<Friend>>(reader.GetString(2)) ?? [];
                }

                userData = new UserData { Secret = _secret, FriendCode = _friendCode, FriendList = _friendList };
                break;
            }
        }
        catch(Exception ex)
        {
            Console.WriteLine($"Exception: {ex.Message}");
        }

        return userData;
    }

    public void Dispose()
    {
        db.Dispose();
        GC.SuppressFinalize(this);
    }
}
