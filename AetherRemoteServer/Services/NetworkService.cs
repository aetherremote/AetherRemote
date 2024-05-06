using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonChatMode;
using AetherRemoteCommon.Domain.CommonFriend;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network.Become;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteServer.Domain;
using Microsoft.AspNetCore.SignalR;
using System.Text;

namespace AetherRemoteServer.Services;

public class NetworkService
{
    // FriendCode -> UserData
    private readonly Dictionary<string, UserData> registeredUsers = [];
    private readonly DatabaseProvider database = new();

    private static readonly bool EnableVerboseLogging = true;

    public ResultWithLogin Login(string connectionId, string secret)
    {
        // Validate Secret
        var userData = database.TryGetUserDataBySecret(secret);
        if (userData == null)
            return new ResultWithLogin(false, "Invalid Secret");

        // Register FriendCode -> UserData
        if (registeredUsers.ContainsKey(userData.FriendCode))
            return new ResultWithLogin(false, "Already Registered");

        userData.ConnectionId = connectionId;
        registeredUsers.Add(userData.FriendCode, userData);

        foreach(var friend in userData.FriendList)
            friend.Online = registeredUsers[friend.FriendCode] != null;

        return new ResultWithLogin(true, "", userData.FriendCode, userData.FriendList);
    }

    public ResultWithMessage Logout(string connectionId)
    {
        var user = registeredUsers.FirstOrDefault(user => user.Value.ConnectionId == connectionId).Value;
        if (user == null)
            return new ResultWithMessage(true, $"ConnectionId {connectionId} may have already been terminated");

        registeredUsers.Remove(user.FriendCode);
        return new ResultWithMessage(true);
    }

    public ResultWithFriends FetchFriendList(string secret)
    {
        var userData = RetrieveOnlineUserBySecret(secret);
        if (userData == null)
            return new ResultWithFriends(false, "Requester not logged in");

        return new ResultWithFriends(true, "", userData.FriendList);
    }

    public ResultWithMessage CreateOrUpdateFriend(string secret, Friend friend)
    {
        var userData = RetrieveOnlineUserBySecret(secret);
        if (userData == null)
            return new ResultWithMessage(false, "Requester not logged in");

        var validFriend = database.TryGetUserDataByFriendCode(friend.FriendCode);
        if (validFriend == null)
            return new ResultWithMessage(false, "Not a real friend code");

        var index = userData.FriendList.FindIndex(fr => fr.FriendCode == friend.FriendCode);
        if (index < 0)
        {
            userData.FriendList.Add(friend); // Create
        }
        else
        {
            userData.FriendList[index] = friend; // Update
        }

        // TODO: Return Friend Online Status
        return new ResultWithMessage(true);
    }

    public ResultWithMessage DeleteFriend(string secret, string friendCode)
    {
        var userData = RetrieveOnlineUserBySecret(secret);
        if (userData == null)
            return new ResultWithMessage(false, "Requester not logged in");

        var result = new ResultWithMessage(true);
        var index = userData.FriendList.FindIndex(friend => friend.FriendCode == friendCode);
        if (index < 0)
        {
            result.Message = "Friend not found";
        }
        else
        {
            userData.FriendList.RemoveAt(index);
        }

        // storageService.SaveUserData();

        return result;
    }

    public ResultWithMessage Become(string secret, List<string> targetFriendCodes, GlamourerApplyType apply, string data, IHubCallerClients clients)
    {
        Log("Become", $"Got new request from {secret}");
        var userData = RetrieveOnlineUserBySecret(secret);
        if (userData == null)
        {
            Log("Become", $"Requester {secret} not logged in");
            return new ResultWithMessage(false, "Requester not logged in");
        }

        foreach (var targetFriendCode in targetFriendCodes)
        {
            Log("Become", $"Processing Emote command for target friend code {targetFriendCode}");
            if (registeredUsers.TryGetValue(targetFriendCode, out var targetUser) == false)
            {
                Log("Become", $"Friend code {targetFriendCode} is offline, skipping");
                continue;
            }

            if (targetUser.ConnectionId == null)
            {
                Log("Become", $"Friend code {targetFriendCode} does not have a connection id set????");
                continue;
            }

            try
            {
                Log("Become", $"Sending command to {targetFriendCode}...");
                var request = new BecomeExecute(userData.FriendCode, data, apply);
                clients.Client(targetUser.ConnectionId).SendAsync(Constants.ApiBecome, request);
                Log("Become", $"Sent command to {targetFriendCode}!");
            }
            catch (Exception ex)
            {
                Log("Become", $"Error sending command to {targetFriendCode}! Error was {ex.Message}");
            }
        }

        return new ResultWithMessage(true);
    }

    public ResultWithMessage Emote(string secret, List<string> targetFriendCodes, string emote, IHubCallerClients clients)
    {
        Log("Emote", $"Got new request from {secret}");
        var userData = RetrieveOnlineUserBySecret(secret);
        if (userData == null)
        {
            Log("Emote", $"Requester {secret} not logged in");
            return new ResultWithMessage(false, "Requester not logged in");
        }
            
        foreach (var targetFriendCode in targetFriendCodes)
        {
            Log("Emote", $"Processing Emote command for target friend code {targetFriendCode}");
            if (registeredUsers.TryGetValue(targetFriendCode, out var targetUser) == false)
            {
                Log("Emote", $"Friend code {targetFriendCode} is offline, skipping");
                continue;
            }
                
            if (targetUser.ConnectionId == null)
            {
                Log("Emote", $"Friend code {targetFriendCode} does not have a connection id set????");
                continue;
            }

            try
            {
                Log("Emote", $"Sending command to {targetFriendCode}...");
                var request = new EmoteExecute(userData.FriendCode, emote);
                clients.Client(targetUser.ConnectionId).SendAsync(Constants.ApiEmote, request);
                Log("Emote", $"Sent command to {targetFriendCode}!");
            }
            catch (Exception ex)
            {
                Log("Emote", $"Error sending command to {targetFriendCode}! Error was {ex.Message}");
            }
        }

        return new ResultWithMessage(true);
    }

    public ResultWithMessage Speak(string secret, List<string> targetFriendCodes, string message, ChatMode chatMode, string? extra, IHubCallerClients clients)
    {
        Log("Speak", $"Got new request from {secret}");
        var userData = RetrieveOnlineUserBySecret(secret);
        if (userData == null)
        {
            Log("Speak", $"Requester {secret} not logged in");
            return new ResultWithMessage(false, "Requester not logged in");
        }

        foreach (var targetFriendCode in targetFriendCodes)
        {
            Log("Speak", $"Processing Emote command for target friend code {targetFriendCode}");
            if (registeredUsers.TryGetValue(targetFriendCode, out var targetUser) == false)
            {
                Log("Speak", $"Friend code {targetFriendCode} is offline, skipping");
                continue;
            }

            if (targetUser.ConnectionId == null)
            {
                Log("Speak", $"Friend code {targetFriendCode} does not have a connection id set????");
                continue;
            }

            try
            {
                Log("Speak", $"Sending command to {targetFriendCode}...");
                var request = new SpeakExecute(userData.FriendCode, message, chatMode, extra);
                clients.Client(targetUser.ConnectionId).SendAsync(Constants.ApiSpeak, request);
                Log("Speak", $"Sent command to {targetFriendCode}!");
            }
            catch (Exception) { }
        }

        return new ResultWithMessage(true);
    }

    // TODO: Technically this function has a blind spot, in that you cannot determine if it was an invalid secret, or the user simply isn't online
    private UserData? RetrieveOnlineUserBySecret(string secret)
    {
        var userData = database.TryGetUserDataBySecret(secret);
        if (userData == null)
            return null;

        if (registeredUsers.TryGetValue(userData.FriendCode, out var registeredUser) == false)
            return null;

        return registeredUser;
    }

    private static void Log(string method, string message)
    {
        if (EnableVerboseLogging == false)
            return;

        var sb = new StringBuilder();
        sb.Append('[');
        sb.Append(method);
        sb.Append("] ");
        sb.Append(message);
        Console.WriteLine(sb.ToString());
    }
}
