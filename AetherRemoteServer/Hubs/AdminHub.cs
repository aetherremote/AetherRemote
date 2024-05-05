using AetherRemoteServer.Domain;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Hubs;

public class AdminHub : Hub
{
    [HubMethodName("Insert")]
    public void Insert(string secret, string friendCode)
    {
        var db = new DatabaseProvider();

        var userData = new UserData
        {
            Secret = secret,
            FriendCode = friendCode,
        };

        db.CreateOrUpdateUserData(userData);
        db.Dispose();
    }

    [HubMethodName("Fetch")]
    public UserData? Fetch(string secret)
    {
        var db = new DatabaseProvider();
        var userData = db.TryGetUserData(secret);
        db.Dispose();
        return userData;
    }
}
