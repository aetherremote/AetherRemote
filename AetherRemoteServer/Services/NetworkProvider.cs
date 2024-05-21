using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteServer.Domain;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Services;

public class NetworkProvider
{
    private const int SecondsRequiredBetweenCommands = 2;

    private readonly DatabaseProvider database = new();
    private readonly ConnectedClientsManager connectedClientsManager = new();

    public ResultWithLogin Login(string connectionId, string secret, IHubCallerClients clients)
    {
        // Validate Secret
        var userData = database.TryGetUserDataBySecret(secret);
        if (userData == null)
            return new ResultWithLogin(false, "Invalid Secret");

        // Registration Status
        if (connectedClientsManager.IsFriendCodeRegistered(userData.FriendCode))
            return new ResultWithLogin(false, "Already Registered");

        // Create Client
        var client = connectedClientsManager.CreateClient(connectionId, userData);

        // Process Friend List
        foreach(var friend in client.Data.FriendList)
        {
            // Check if friend is online
            var friendClient = connectedClientsManager.GetConnectedClient(friend.FriendCode);
            if (friendClient == null)
            {
                friend.Online = false;
                continue;
            }

            // Set friend online status to true
            friend.Online = true;

            // Send a message to that friend that you are online
            SendOnlineStatus(client, friendClient, true, clients);
        }

        return new ResultWithLogin(true, "", client.Data.FriendCode, client.Data.FriendList);
    }

    public ResultWithMessage Logout(string connectionId, IHubCallerClients clients)
    {
        // Retrieve disconnected client's connected client (which is silly!!!)
        var client = connectedClientsManager.GetConnectedClientByConnectionId(connectionId);
        if (client == null)
            return new ResultWithMessage(true, $"ConnectionId {connectionId} may have already been terminated");

        foreach(var friend in client.Data.FriendList)
        {
            // Check if friend is online
            var friendClient = connectedClientsManager.GetConnectedClient(friend.FriendCode);
            if (friendClient == null)
                continue;

            // Send a message to that friend that you are offline
            SendOnlineStatus(client, friendClient, false, clients);
        }

        connectedClientsManager.RemoveConnectedClient(client.Data.FriendCode);
        return new ResultWithMessage(true);
    }

    public ResultWithFriends FetchFriendList(string secret)
    {
        // Validate Online
        var client = connectedClientsManager.GetConnectedClientBySecret(secret);
        if (client == null)
            return new ResultWithFriends(false, "Not Logged In");

        return new ResultWithFriends(true, "", client.Data.FriendList);
    }

    public ResultWithOnlineStatus CreateOrUpdateFriend(string secret, Friend friendToCreateOrUpdate)
    {
        // Validate Online
        var client = connectedClientsManager.GetConnectedClientBySecret(secret);
        if (client == null)
            return new ResultWithOnlineStatus(false, "Not Logged In");

        // Validate FriendCode to Create or Update
        var friendUserData = database.TryGetUserDataByFriendCode(friendToCreateOrUpdate.FriendCode);
        if (friendUserData == null)
            return new ResultWithOnlineStatus(false, "Invalid Friend Code");

        // Create or Update
        var friendIndex = client.Data.FriendList.FindIndex(friend => friend.FriendCode == friendToCreateOrUpdate.FriendCode);
        if (friendIndex < 0)
            client.Data.FriendList.Add(friendToCreateOrUpdate);
        else
            client.Data.FriendList[friendIndex] = friendToCreateOrUpdate;

        // Reflect changes in database
        database.CreateOrUpdateUserData(client.Data);

        return new ResultWithOnlineStatus(true, "", connectedClientsManager.IsFriendCodeRegistered(friendToCreateOrUpdate.FriendCode));
    }

    public ResultWithMessage DeleteFriend(string secret, string friendCodeToDelete)
    {
        // Validate Online
        var client = connectedClientsManager.GetConnectedClientBySecret(secret);
        if (client == null)
            return new ResultWithMessage(false, "Not Logged In");

        // Create return result
        var result = new ResultWithMessage(true);

        // Preform potential removals
        var numberOfRemovedFriends = client.Data.FriendList.RemoveAll(friend => friend.FriendCode == friendCodeToDelete);
        if (numberOfRemovedFriends == 0)
            result.Message = "No Operation";

        // Reflect changes in database
        database.CreateOrUpdateUserData(client.Data);

        return result;
    }

    public ResultWithMessage Become(string secret, List<string> targetFriendCodes, GlamourerApplyType apply, string data, IHubCallerClients clients)
    {
        // Validate Online
        var client = connectedClientsManager.GetConnectedClientBySecret(secret);
        if (client == null)
            return new ResultWithMessage(false, "Not Logged In");

        // Check if spamming
        if (IsClientSpamming(client))
            return new ResultWithMessage(false, "Spam");

        // Temporarily Mass Control Block
        if (targetFriendCodes.Count != 1)
            return new ResultWithMessage(false, "Mass Control not supported");

        // Iterate over all target friends
        foreach (var targetFriendCode in targetFriendCodes)
        {
            // Check if friend is online
            var targetClient = connectedClientsManager.GetConnectedClient(targetFriendCode);
            if (targetClient == null)
                continue;

            // Check if target is friends with sender
            if (targetClient.IsFriendsWith(client) == false)
                continue;

            try
            {
                // Try Send Become Command
                var request = new BecomeExecute(client.Data.FriendCode, data, apply);
                clients.Client(targetClient.ConnectionId).SendAsync(Constants.ApiBecome, request);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error sending become command to {targetFriendCode}! Error was {ex.Message}");
            }
        }

        client.LastCommandTimestamp = DateTime.UtcNow;
        return new ResultWithMessage(true);
    }

    public ResultWithMessage Emote(string secret, List<string> targetFriendCodes, string emote, IHubCallerClients clients)
    {
        // Validate Online
        var client = connectedClientsManager.GetConnectedClientBySecret(secret);
        if (client == null)
            return new ResultWithMessage(false, "Not Logged In");

        // Check if spamming
        if (IsClientSpamming(client))
            return new ResultWithMessage(false, "Spam");

        // Temporarily Mass Control Block
        if (targetFriendCodes.Count != 1)
            return new ResultWithMessage(false, "Mass Control not supported");

        // Iterate over all target friends
        foreach (var targetFriendCode in targetFriendCodes)
        {
            // Check if friend is online
            var targetClient = connectedClientsManager.GetConnectedClient(targetFriendCode);
            if (targetClient == null)
                continue;

            // Check if target is friends with sender
            if (targetClient.IsFriendsWith(client) == false)
                continue;

            try
            {
                // Try Send Emote Command
                var request = new EmoteExecute(client.Data.FriendCode, emote);
                clients.Client(targetClient.ConnectionId).SendAsync(Constants.ApiEmote, request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending emote command to {targetFriendCode}! Error was {ex.Message}");
            }
        }

        client.LastCommandTimestamp = DateTime.UtcNow;
        return new ResultWithMessage(true);
    }

    public ResultWithMessage Speak(string secret, List<string> targetFriendCodes, string message, ChatMode chatMode, string? extra, IHubCallerClients clients)
    {
        // Validate Online
        var client = connectedClientsManager.GetConnectedClientBySecret(secret);
        if (client == null)
            return new ResultWithMessage(false, "Not Logged In");

        // Check if spamming
        if (IsClientSpamming(client))
            return new ResultWithMessage(false, "Spam");

        // Temporarily Mass Control Block
        if (targetFriendCodes.Count != 1)
            return new ResultWithMessage(false, "Mass Control not supported");

        // Iterate over all target friends
        foreach (var targetFriendCode in targetFriendCodes)
        {
            // Check if friend is online
            var targetClient = connectedClientsManager.GetConnectedClient(targetFriendCode);
            if (targetClient == null)
                continue;

            // Check if target is friends with sender
            if (targetClient.IsFriendsWith(client) == false)
                continue;

            try
            {
                // Try Send Speak Command
                var request = new SpeakExecute(client.Data.FriendCode, message, chatMode, extra);
                clients.Client(targetClient.ConnectionId).SendAsync(Constants.ApiSpeak, request);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending speak command to {targetFriendCode}! Error was {ex.Message}");
            }
        }

        client.LastCommandTimestamp = DateTime.UtcNow;
        return new ResultWithMessage(true);
    }

    /// <summary>
    /// Attempts to send a message to connected client indicating online status
    /// </summary>
    /// <param name="senderClient"></param>
    /// <param name="receptientClient"></param>
    /// <param name="hubCallerClients"></param>
    private static void SendOnlineStatus(ConnectedClient senderClient, ConnectedClient receptientClient, bool online, IHubCallerClients hubCallerClients)
    {
        try
        {
            var request = new OnlineStatusExecute(senderClient.Data.FriendCode, online);
            hubCallerClients.Client(receptientClient.ConnectionId).SendAsync(Constants.ApiOnlineStatus, request);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending online status to {receptientClient.Data.FriendCode}! Exception was: {ex.Message}");
        }
    }

    private static bool IsClientSpamming(ConnectedClient client)
    {
        return (DateTime.UtcNow - client.LastCommandTimestamp).TotalSeconds < SecondsRequiredBetweenCommands;
    }
}
