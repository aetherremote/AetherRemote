using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Services;
using AetherRemoteServer.Services.Database;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Managers;

/// <summary>
///     TODO
/// </summary>
public class ForwardedRequestManager(DatabaseService database, PresenceService presence, ILogger<ForwardedRequestManager> logger)
{
    private static readonly TimeSpan TimeOutDuration = TimeSpan.FromSeconds(8);

    /// <summary>
    ///     TODO
    /// </summary>
    public async Task<ActionResponse> CheckPermissionsAndSend(
        string senderFriendCode, 
        List<string> targetFriendCodes, 
        string method, 
        ResolvedPermissions required, 
        ActionCommand request, 
        IHubCallerClients clients)
    {
        var tasks = new Task<ActionResult<Unit>>[targetFriendCodes.Count];
        for (var i = 0; i < targetFriendCodes.Count; i++)
        {
            var target = targetFriendCodes[i];
            var (client, failure) = await EvaluateTargetAsync(senderFriendCode, target, required, clients);
            
            // If there is not a failure, proceed with the call, otherwise return the failure
            tasks[i] = failure is null
                ? ForwardRequestWithTimeout<Unit>(method, client, request)
                : Task.FromResult(failure);
        }

        var completed = await Task.WhenAll(tasks);
        var results = new Dictionary<string, ActionResultEc>(targetFriendCodes.Count);
        for (var i = 0; i < targetFriendCodes.Count; i++)
            results[targetFriendCodes[i]] = completed[i].Result;

        return new ActionResponse(ActionResponseEc.Success, results);
    }
    
    /// <summary>
    ///     TODO
    /// </summary>
    public async Task<PossessionResponse> CheckPossessionAndInvoke(
        string senderFriendCode, 
        string targetFriendCode, 
        string method, 
        ResolvedPermissions required, 
        ActionCommand request, 
        IHubCallerClients clients)
    {
        if (presence.TryGet(targetFriendCode) is not { } target)
            return new PossessionResponse(PossessionResponseEc.TargetOffline, PossessionResultEc.Uninitialized);
        
        if (await database.GetSinglePermissions(targetFriendCode, senderFriendCode) is not { } permissions)
            return new PossessionResponse(PossessionResponseEc.TargetNotFriends, PossessionResultEc.Uninitialized);

        var global = await database.GetGlobalPermissions(targetFriendCode);
        var resolved = PermissionResolver.Resolve(global, permissions);
        
        if (HasRequiredPermissions(resolved, required) is false)
            return new PossessionResponse(PossessionResponseEc.LacksPermissions, PossessionResultEc.Uninitialized);

        var client = clients.Client(target.ConnectionId);
        using var token = new CancellationTokenSource(TimeOutDuration);
        try
        {
            var response = await client.InvokeAsync<PossessionResultEc>(method, request, token.Token);
            return new PossessionResponse(PossessionResponseEc.Success, response);
        }
        catch (OperationCanceledException)
        {
            return new PossessionResponse(PossessionResponseEc.Timeout, PossessionResultEc.Uninitialized);
        }
        catch (Exception e)
        {
            logger.LogError("{Error}", e);
            return new PossessionResponse(PossessionResponseEc.Unknown, PossessionResultEc.Uninitialized);
        }
    }
    
    private async Task<(ISingleClientProxy client, ActionResult<Unit>? result)> EvaluateTargetAsync(string senderFriendCode, string targetFriendCode, ResolvedPermissions required, IHubCallerClients clients)
    {
        if (presence.TryGet(targetFriendCode) is not { } target)
            return (null!, ActionResultBuilder.Fail(ActionResultEc.TargetOffline));
        
        if (await database.GetSinglePermissions(targetFriendCode, senderFriendCode) is not { } permissions)
            return (null!, ActionResultBuilder.Fail(ActionResultEc.TargetNotFriends));

        var global = await database.GetGlobalPermissions(targetFriendCode);
        var resolved = PermissionResolver.Resolve(global, permissions);
        
        if (HasRequiredPermissions(resolved, required) is false)
            return  (null!, ActionResultBuilder.Fail(ActionResultEc.TargetHasNotGrantedSenderPermissions));

        return (clients.Client(target.ConnectionId), null);
    }

    private static bool HasRequiredPermissions(ResolvedPermissions granted, ResolvedPermissions required)
    {
        return (granted.Primary & required.Primary) == required.Primary
               && (granted.Speak & required.Speak) == required.Speak
               && (granted.Elevated & required.Elevated) == required.Elevated;
    }
    
    /// <summary>
    ///     Forwards a request to a client with a timeout of 8 seconds
    /// </summary>
    public static async Task<ActionResult<T>> ForwardRequestWithTimeout<T>(string method, ISingleClientProxy client, ActionCommand forward)
    {
        using var token = new CancellationTokenSource(TimeOutDuration);

        try
        {
            return await client.InvokeAsync<ActionResult<T>>(method, forward, token.Token);
        }
        catch (OperationCanceledException)
        {
            return ActionResultBuilder.Fail<T>(ActionResultEc.TargetTimeout);
        }
        catch
        {
            return ActionResultBuilder.Fail<T>(ActionResultEc.Unknown);
        }
    }
}