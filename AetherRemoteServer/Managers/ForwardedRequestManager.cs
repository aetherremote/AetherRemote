using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.New;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network;
using AetherRemoteCommon.V2.Domain.Network.BodySwap;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Managers;

/// <summary>
///     TODO
/// </summary>
/// <param name="connections"></param>
/// <param name="database"></param>
/// <param name="logger"></param>
public class ForwardedRequestManager(IConnectionsService connections, IDatabaseService database, ILogger<ForwardedRequestManager> logger) : IForwardedRequestManager
{
    private static readonly TimeSpan TimeOutDuration = TimeSpan.FromSeconds(8);
    
    /// <summary>
    ///     TODO
    /// </summary>
    public async Task<ActionResponse> Send(string senderFriendCode, List<string> targetFriendCodes, PrimaryRequestInfo requestInfo, IHubCallerClients clients)
    {
        var results = new Dictionary<string, ActionResultEc>();
        var pending = new Task<ActionResult<Unit>>[targetFriendCodes.Count];
        for (var i = 0; i < targetFriendCodes.Count; i++)
        {
            var targetFriendCode =  targetFriendCodes[i];
            var connectionIdResult = await TryGetAuthorizedConnectionAsync(senderFriendCode, targetFriendCode, requestInfo.Permissions);

            if (connectionIdResult.Result is not ActionResultEc.Success)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(connectionIdResult.Result));
                continue;
            }
            
            if (connectionIdResult.Value is not { } connectionId)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.ValueNotSet));
                continue;
            }
            
            try
            {
                var client = clients.Client(connectionId);
                pending[i] = AwaitResponsesWithTimeout<Unit>(requestInfo.Method, client, requestInfo.Request);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", senderFriendCode, targetFriendCode, e.Message);
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.Unknown));
            }
        }
        
        var completed = await Task.WhenAll(pending).ConfigureAwait(false);
        for(var i = 0; i < completed.Length; i++)
            results.Add(targetFriendCodes[i], completed[i].Result);

        return new ActionResponse(results);
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public async Task<ActionResponse> Send(string senderFriendCode, List<string> targetFriendCodes, SpeakRequestInfo requestInfo, IHubCallerClients clients)
    {
        var results = new Dictionary<string, ActionResultEc>();
        var pending = new Task<ActionResult<Unit>>[targetFriendCodes.Count];
        for (var i = 0; i < targetFriendCodes.Count; i++)
        {
            var targetFriendCode =  targetFriendCodes[i];
            var connectionIdResult = await TryGetAuthorizedConnectionAsync(senderFriendCode, targetFriendCode, requestInfo.Permissions);

            if (connectionIdResult.Result is not ActionResultEc.Success)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(connectionIdResult.Result));
                continue;
            }
            
            if (connectionIdResult.Value is not { } connectionId)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.ValueNotSet));
                continue;
            }
            
            try
            {
                var client = clients.Client(connectionId);
                pending[i] = AwaitResponsesWithTimeout<Unit>(requestInfo.Method, client, requestInfo.Request);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", senderFriendCode, targetFriendCode, e.Message);
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.Unknown));
            }
        }
        
        var completed = await Task.WhenAll(pending).ConfigureAwait(false);
        for(var i = 0; i < completed.Length; i++)
            results.Add(targetFriendCodes[i], completed[i].Result);

        return new ActionResponse(results);
    }

    /// <summary>
    ///     TODO
    /// </summary>
    public async Task<BodySwapResponse> SendBodySwap(string senderFriendCode, List<string> targetFriendCodes, List<string> characterNames, CharacterAttributes attributes, IHubCallerClients clients)
    {
        var permissions = SwapAttributesToPrimaryPermissions(attributes);
        var results = new Dictionary<string, ActionResultEc>();
        var pending = new Task<ActionResult<Unit>>[targetFriendCodes.Count];
        for (var i = 0; i < targetFriendCodes.Count; i++)
        {
            var targetFriendCode =  targetFriendCodes[i];
            var connectionIdResult = await TryGetAuthorizedConnectionAsync(senderFriendCode, targetFriendCode, permissions);

            if (connectionIdResult.Result is not ActionResultEc.Success)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(connectionIdResult.Result));
                continue;
            }
            
            if (connectionIdResult.Value is not { } connectionId)
            {
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.ValueNotSet));
                continue;
            }
            
            var request = new BodySwapForwardedRequest(senderFriendCode, characterNames[i], attributes);
            
            try
            {
                var client = clients.Client(connectionId);
                pending[i] = AwaitResponsesWithTimeout<Unit>(HubMethod.BodySwap, client, request);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", senderFriendCode, targetFriendCode, e.Message);
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.Unknown));
            }
        }
        
        var completed = await Task.WhenAll(pending).ConfigureAwait(false);
        for(var i = 0; i < completed.Length; i++)
            results.Add(targetFriendCodes[i], completed[i].Result);
        
        return new BodySwapResponse(results, targetFriendCodes.Count < characterNames.Count ? characterNames[^1] : null);
    }

    /// <summary>
    ///     TODO
    /// </summary>
    private async Task<ActionResult<string>> TryGetAuthorizedConnectionAsync(string sender, string target, PrimaryPermissions2 permissions)
    {
        if (connections.TryGetClient(target) is not { } connectedClient)
            return ActionResultBuilder.Fail<string>(ActionResultEc.TargetOffline);
        
        var targetPermissions = await database.GetPermissions(target);
        if (targetPermissions.Permissions.TryGetValue(sender, out var permissionsGranted) is false)
            return ActionResultBuilder.Fail<string>(ActionResultEc.TargetNotFriends);

        if (permissions == PrimaryPermissions2.None || (permissionsGranted.Primary & permissions) == permissions)
            return ActionResultBuilder.Ok(connectedClient.ConnectionId);
        
        return ActionResultBuilder.Fail<string>(ActionResultEc.TargetHasNotGrantedSenderPermissions);
    }
    
    /// <summary>
    ///     TODO
    /// </summary>
    private async Task<ActionResult<string>> TryGetAuthorizedConnectionAsync(string sender, string target, SpeakPermissions2 permissions)
    {
        if (connections.TryGetClient(target) is not { } connectedClient)
            return ActionResultBuilder.Fail<string>(ActionResultEc.TargetOffline);
        
        var targetPermissions = await database.GetPermissions(target);
        if (targetPermissions.Permissions.TryGetValue(sender, out var permissionsGranted) is false)
            return ActionResultBuilder.Fail<string>(ActionResultEc.TargetNotFriends);

        if (permissions == SpeakPermissions2.None || (permissionsGranted.Speak & permissions) == permissions)
            return ActionResultBuilder.Ok(connectedClient.ConnectionId);
        
        return ActionResultBuilder.Fail<string>(ActionResultEc.TargetHasNotGrantedSenderPermissions);
    }
    
    /// <summary>
    ///     TODO
    /// </summary>
    private static Task<ActionResult<T>> AwaitResponsesWithTimeout<T>(
        string methodName,
        ISingleClientProxy clientProxy, 
        ForwardedActionRequest forwardedRequest)
    {
        var token = new CancellationTokenSource(TimeOutDuration);
        return clientProxy.InvokeAsync<ActionResult<T>>(methodName, forwardedRequest, token.Token)
            .ContinueWith(task =>
            {
                if (task.IsCanceled || token.IsCancellationRequested)
                    return ActionResultBuilder.Fail<T>(ActionResultEc.TargetTimeout);

                return task.IsFaulted
                    ? ActionResultBuilder.Fail<T>(ActionResultEc.Unknown)
                    : task.Result;
            }, TaskContinuationOptions.ExecuteSynchronously);
    }
    
    /// <summary>
    ///     TODO
    /// </summary>
    private static PrimaryPermissions2 SwapAttributesToPrimaryPermissions(CharacterAttributes attributes)
    {
        var permissions = PrimaryPermissions2.Twinning;
        if ((attributes & CharacterAttributes.Mods) == CharacterAttributes.Mods)
            permissions |= PrimaryPermissions2.Mods;

        if ((attributes & CharacterAttributes.Moodles) == CharacterAttributes.Moodles)
            permissions |= PrimaryPermissions2.Moodles;

        if ((attributes & CharacterAttributes.CustomizePlus) == CharacterAttributes.CustomizePlus)
            permissions |= PrimaryPermissions2.CustomizePlus;

        return permissions;
    }
}