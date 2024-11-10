using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Commands;
using AetherRemoteServer.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Services;

public class NetworkService(DatabaseService databaseService, ILogger<NetworkService> logger)
{
    // Injected
    private readonly DatabaseService databaseService = databaseService;
    private readonly ILogger<NetworkService> logger = logger;

    // Random implementation for Derangement operations
    private static readonly Random Random = new();

    /// <summary>
    /// Creates or updates a user from the user database
    /// </summary>
    public async Task<CreateOrUpdateUserResponse> CreateOrUpdateUser(CreateOrUpdateUserRequest request)
    {
        var result = await databaseService.CreateOrUpdateUser(request.FriendCode, request.Secret, request.IsAdmin);
        return new CreateOrUpdateUserResponse(result == 1);
    }

    /// <summary>
    /// Deletes a user from the user database
    /// </summary>
    public async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request)
    {
        var result = await databaseService.DeleteUser(request.FriendCode);
        return new DeleteUserResponse(result == 1);
    }

    /// <summary>
    /// Attempts to get a user from the user database
    /// </summary>
    public async Task<GetUserResponse> GetUser(GetUserRequest request)
    {
        var result = await databaseService.GetUser(request.FriendCode);
        return new GetUserResponse(result != null, "", result?.FriendCode, result?.Secret, result?.IsAdmin);
    }

    /// <summary>
    /// Retrieves data about the calling user
    /// </summary>
    public async Task<LoginDetailsResponse> LoginDetails(string friendCode, LoginDetailsRequest request)
    {
        var permissionsGrantedToOthers = await databaseService.GetPermissions(friendCode);
        var permissionsGrantedByOthers = new Dictionary<string, UserPermissions>();
        foreach(var kvp in permissionsGrantedToOthers)
        {
            if (PrimaryHub.ActiveUserConnections.ContainsKey(kvp.Key) == false)
                continue;

            var permissionsGrantedByFriendToOthers = await databaseService.GetPermissions(kvp.Key);
            if (permissionsGrantedByFriendToOthers.TryGetValue(friendCode, out var permissions))
                permissionsGrantedByOthers[kvp.Key] = permissions;
        }

        return new LoginDetailsResponse(true, friendCode, permissionsGrantedToOthers, permissionsGrantedByOthers);
    }

    /// <summary>
    /// Creates or updates permissions a user has set for another
    /// </summary>
    public async Task<CreateOrUpdatePermissionsResponse> CreateOrUpdatePermissions(string friendCode, CreateOrUpdatePermissionsRequest request, IHubCallerClients clients)
    {
        if (friendCode == request.TargetCode)
            return new CreateOrUpdatePermissionsResponse(false, false, "Cannot add yourself!");

        var (rows, message) = await databaseService.CreateOrUpdatePermissions(friendCode, request.TargetCode, request.Permissions);
        var online = false;
        if (PrimaryHub.ActiveUserConnections.TryGetValue(request.TargetCode, out var connection))
        {
            try
            {
                var command = new UpdateLocalPermissionsCommand(friendCode, request.Permissions);
                _ = clients.Client(connection.ConnectionId).SendAsync(Network.Commands.UpdateLocalPermissions, command);
                online = true;
            }
            catch (Exception ex)
            {
                logger.LogError("Exception while updating permissions for online clients {Exception}", ex);
                return new CreateOrUpdatePermissionsResponse(false, false, ex.Message);
            }
        }

        return new CreateOrUpdatePermissionsResponse(rows == 1, online, message);
    }

    /// <summary>
    /// Deletes the permissions a user has set for another
    /// </summary>
    public async Task<DeletePermissionsResponse> DeletePermissions(string friendCode, DeletePermissionsRequest request)
    {
        var (rows, message) = await databaseService.DeletePermissions(friendCode, request.TargetCode);
        return new DeletePermissionsResponse(rows == 1, message);
    }

    /// <summary>
    /// Handles querying body data, and issuing body swap commands to all target friend codes
    /// </summary>
    public async Task<BodySwapResponse> BodySwap(string friendCode, BodySwapRequest request, IHubCallerClients clients)
    {
        if (IsUserSpamming(friendCode))
            return new(false, null, null, "Spamming! Slow down!");

        if (request.TargetFriendCodes.Count < 2)
        {
            if (request.CharacterData is null || request.TargetFriendCodes.Count == 0)
                return new(false, null, null, "Minimum targets not met.");
            else if (request.TargetFriendCodes[0] == friendCode)
                return new(false, null, null, "Cannot bodyswap with just yourself");
        }

        var cancellationToken = new CancellationTokenSource();
        var taskList = new Task<BodySwapQueryResponse>[request.TargetFriendCodes.Count];
        var connectionIds = new string[request.TargetFriendCodes.Count];
        for (var i = 0; i < request.TargetFriendCodes.Count; i++)
        {
            // Target not online
            if (PrimaryHub.ActiveUserConnections.TryGetValue(request.TargetFriendCodes[i], out var targetUser) == false)
            {
                cancellationToken.Cancel();
                return new(false, null, null, "One or more target friends are offline");
            }

            // Not friends with
            var targetPermissions = await databaseService.GetPermissions(request.TargetFriendCodes[i]);
            if (targetPermissions.TryGetValue(friendCode, out var permissionsGrantedToFriendCode) == false)
            {
                cancellationToken.Cancel();
                return new(false, null, null, "You are not friends with all your targets");
            }

            // Has valid transform permissions
            if (PermissionChecker.HasValidTransformPermissions(GlamourerApplyFlag.Customization | GlamourerApplyFlag.Equipment, permissionsGrantedToFriendCode) == false)
            {
                cancellationToken.Cancel();
                return new(false, null, null, "You do not have valid permissions with all your friends");
            }

            try
            {
                // Issue query requests for body data
                var query = new BodySwapQueryRequest(friendCode, request.SwapMods);
                taskList[i] = clients.Client(targetUser.ConnectionId).InvokeAsync<BodySwapQueryResponse>(Network.BodySwapQuery, query, cancellationToken.Token);
                connectionIds[i] = targetUser.ConnectionId;
            }
            catch (Exception ex)
            {
                cancellationToken.Cancel();
                logger.LogError("Exception while querying body data: {Exception}", ex);
                return new(false, null, null, ex.Message);
            }
        }

        // Create time out task
        var timeoutTask = Task.Delay(8000); // 8 seconds * 1000 ms
        var pendingQueryTasks = Task.WhenAll(taskList);

        // Wait for either to finish
        var resultingTask = await Task.WhenAny(pendingQueryTasks, timeoutTask);
        if (resultingTask == timeoutTask)
        {
            cancellationToken.Cancel();
            return new(false, null, null, "Body swap timed out");
        }

        // WhenAny should take care of this, but just in case...
        var queryResponses = await pendingQueryTasks;

        // Convert them in the order we sent the requests
        var bodyData = new List<BodyData>();
        for (var i = 0; i < queryResponses.Length; i++)
        {
            var response = queryResponses[i];
            if (response.CharacterData is null || (request.SwapMods && response.CharacterName is null))
            {
                cancellationToken.Cancel();
                return new(false, null, null, "One or more targets are unable to swap bodies at this time");
            }

            bodyData.Add(new BodyData(response.CharacterName, response.CharacterData));
        }

        // Add extra body data for sender client
        if (request.CharacterData is not null)
            bodyData.Add(new BodyData(request.CharacterName, request.CharacterData));

        // Shuffle all body data, then distribute it
        var derangedBodyData = Derange(bodyData);
        for (var i = 0; i < request.TargetFriendCodes.Count; i++)
        {
            try
            {
                // Issue new body data 
                var data = derangedBodyData[i];
                var command = new BodySwapCommand(friendCode, data.CharacterName, data.CharacterData);
                _ = clients.Client(connectionIds[i]).SendAsync(Network.Commands.BodySwap, command);
            }
            catch (Exception ex)
            {
                cancellationToken.Cancel();
                logger.LogError("Exception while sending body data: {Exception}", ex);
                return new(false, null, null, ex.Message);
            }
        }

        if (request.CharacterData is null)
            return new BodySwapResponse(true);

        var lastBody = derangedBodyData[^1];
        return new BodySwapResponse(true, lastBody.CharacterName, lastBody.CharacterData);
    }

    /// <summary>
    /// Handles issueing emote commands to all target friend codes
    /// </summary>
    public async Task<EmoteResponse> Emote(string friendCode, EmoteRequest request, IHubCallerClients clients)
    {
        if (IsUserSpamming(friendCode))
            return new(false, "Spamming! Slow down!");

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
            return new(false, $"You may only target up to {Constraints.MaximumTargetsForInGameOperations} friends for commands that affect the game");

        foreach (var targetFriendCode in request.TargetFriendCodes)
        {
            // Target not online
            if (PrimaryHub.ActiveUserConnections.TryGetValue(targetFriendCode, out var targetUser) == false)
                continue;

            // Not friends with
            var targetPermissions = await databaseService.GetPermissions(targetFriendCode);
            if (targetPermissions.TryGetValue(friendCode, out var permissionsGrantedToFriendCode) == false)
                continue;

            // Has valid transform permissions
            if (PermissionChecker.HasValidEmotePermissions(permissionsGrantedToFriendCode) == false)
                continue;

            try
            {
                var command = new EmoteCommand(friendCode, request.Emote, request.DisplayLogMessage);
                _ = clients.Client(targetUser.ConnectionId).SendAsync(Network.Commands.Emote, command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception sending emote command to {targetFriendCode}! {ex.Message}");
            }
        }

        PrimaryHub.ActiveUserConnections[friendCode].LastAction = DateTime.Now;
        return new(true);
    }

    /// <summary>
    /// Handles issueing speak commands to all target friend codes
    /// </summary>
    public async Task<SpeakResponse> Speak(string friendCode, SpeakRequest request, IHubCallerClients clients)
    {
        if (IsUserSpamming(friendCode))
            return new(false, "Spamming! Slow down!");

        if (request.TargetFriendCodes.Count > Constraints.MaximumTargetsForInGameOperations)
            return new(false, $"You may only target up to {Constraints.MaximumTargetsForInGameOperations} friends for commands that affect the game");

        foreach (var targetFriendCode in request.TargetFriendCodes)
        {
            // Target not online
            if (PrimaryHub.ActiveUserConnections.TryGetValue(targetFriendCode, out var targetUser) == false)
                continue;

            // Not friends with
            var targetPermissions = await databaseService.GetPermissions(targetFriendCode);
            if (targetPermissions.TryGetValue(friendCode, out var permissionsGrantedToFriendCode) == false)
                continue;

            // Has valid transform permissions
            var linkshellNumber = ParseLinkshellNumber(request.Extra);
            if (PermissionChecker.HasValidSpeakPermissions(request.ChatMode, permissionsGrantedToFriendCode, linkshellNumber) == false)
                continue;

            try
            {
                var command = new SpeakCommand(friendCode, request.Message, request.ChatMode, request.Extra);
                _ = clients.Client(targetUser.ConnectionId).SendAsync(Network.Commands.Speak, command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception sending speak command to {targetFriendCode}! {ex.Message}");
            }
        }

        PrimaryHub.ActiveUserConnections[friendCode].LastAction = DateTime.Now;
        return new(true);
    }

    /// <summary>
    /// Handles issuing transform commands to all target friend codes
    /// </summary>
    public async Task<TransformResponse> Transform(string friendCode, TransformRequest request, IHubCallerClients clients)
    {
        if (IsUserSpamming(friendCode))
            return new(false, "Spamming! Slow down!");

        foreach (var targetFriendCode in request.TargetFriendCodes)
        {
            // Target not online
            if (PrimaryHub.ActiveUserConnections.TryGetValue(targetFriendCode, out var targetUser) == false)
                continue;

            // Not friends with
            var targetPermissions = await databaseService.GetPermissions(targetFriendCode);
            if (targetPermissions.TryGetValue(friendCode, out var permissionsGrantedToFriendCode) == false)
                continue;

            // Has valid transform permissions
            if (PermissionChecker.HasValidTransformPermissions(request.ApplyType, permissionsGrantedToFriendCode) == false)
                continue;

            try
            {
                var command = new TransformCommand(friendCode, request.GlamourerData, request.ApplyType);
                _ = clients.Client(targetUser.ConnectionId).SendAsync(Network.Commands.Transform, command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception sending transform command to {targetFriendCode}! {ex.Message}");
            }
        }

        PrimaryHub.ActiveUserConnections[friendCode].LastAction = DateTime.Now;
        return new(true);
    }

    public async Task<RevertResponse> Revert(string friendCode, RevertRequest request, IHubCallerClients clients)
    {
        if (IsUserSpamming(friendCode))
            return new(false, "Spamming! Slow down!");

        foreach (var targetFriendCode in request.TargetFriendCodes)
        {
            // Target not online
            if (PrimaryHub.ActiveUserConnections.TryGetValue(targetFriendCode, out var targetUser) == false)
                continue;

            // Not friends with
            var targetPermissions = await databaseService.GetPermissions(targetFriendCode);
            if (targetPermissions.TryGetValue(friendCode, out var permissionsGrantedToFriendCode) == false)
                continue;

            // Has valid transform permissions
            if (PermissionChecker.HasAnyTransformPermissions(permissionsGrantedToFriendCode) == false)
                continue;

            try
            {
                var command = new RevertCommand(friendCode, request.RevertType);
                _ = clients.Client(targetUser.ConnectionId).SendAsync(Network.Commands.Revert, command);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception sending revert command to {targetFriendCode}! {ex.Message}");
            }
        }

        PrimaryHub.ActiveUserConnections[friendCode].LastAction = DateTime.Now;
        return new(true);
    }

    /// <summary>
    /// Update all the users who are defined in user's permissions that user's online status has changed
    /// </summary>
    public async void UpdateOnlineStatus(string friendCode, bool online, IHubCallerClients clients)
    {
        var permissionsGrantedToOthers = await databaseService.GetPermissions(friendCode).ConfigureAwait(false);
        foreach (var kvp in permissionsGrantedToOthers)
        {
            if (PrimaryHub.ActiveUserConnections.TryGetValue(kvp.Key, out var user) == false)
                continue;

            try
            {
                var request = new UpdateOnlineStatusCommand(friendCode, online, online ? kvp.Value : UserPermissions.None);
                _ = clients.Client(user.ConnectionId).SendAsync(Network.Commands.UpdateOnlineStatus, request);
            }
            catch (Exception ex)
            {
                logger.LogError("Exception while updating online status {Exception}", ex);
                return;
            }
        }
    }

    /// <summary>
    /// Checks if a user is spamming or not
    /// </summary>
    private static bool IsUserSpamming(string friendCode)
    {
        return (DateTime.UtcNow - PrimaryHub.ActiveUserConnections[friendCode].LastAction).TotalSeconds < Constraints.ExternalCommandCooldownInSeconds;
    }

    /// <summary>
    /// Parses a linkshell number from a string to an int
    /// </summary>
    private static int ParseLinkshellNumber(string? linkshellNumber)
    {
        if (linkshellNumber == null) return -1;
        return int.TryParse(linkshellNumber, out var result) ? result : -1;
    }

    /// <summary>
    /// Deranges a <see cref="List{T}"/> of objects. <see href="https://en.wikipedia.org/wiki/Derangement"/>
    /// </summary>
    private static List<T> Derange<T>(List<T> input)
    {
        if (input.Count < 2) // No Swap
            return input;

        if (input.Count == 2) // Quick Swap
            return [input[1], input[0]];

        var result = new List<T>(input);
        var n = result.Count;

        var deranged = true;
        do
        {
            for (var i = n - 1; i > 0; i--)
            {
                var j = Random.Next(0, i + 1);
                (result[i], result[j]) = (result[j], result[i]);
            }

            for (var i = 0; i < n; i++)
            {
                if (EqualityComparer<T>.Default.Equals(result[i], input[i]))
                {
                    deranged = false;
                    break;
                }
            }
        }
        while (deranged == false);

        return result;
    }

    private class BodyData(string? characterName, string characterData)
    {
        public readonly string? CharacterName = characterName;
        public readonly string CharacterData = characterData;
    }
}
