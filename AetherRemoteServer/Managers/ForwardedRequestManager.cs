using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Managers;

/// <summary>
///     <inheritdoc cref="IForwardedRequestManager"/>
/// </summary>
public class ForwardedRequestManager(IDatabaseService database, IPresenceService presence, ILogger<ForwardedRequestManager> logger) : IForwardedRequestManager
{
    private static readonly TimeSpan TimeOutDuration = TimeSpan.FromSeconds(8);

    /// <summary>
    ///     <inheritdoc cref="IForwardedRequestManager.CheckPermissionsAndSend"/>
    /// </summary>
    public async Task<ActionResponse> CheckPermissionsAndSend(
        string senderFriendCode, 
        List<string> targetFriendCodes, 
        string method, 
        UserPermissions required, 
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
    ///     <inheritdoc cref="IForwardedRequestManager.CheckPossessionAndSend"/>
    /// </summary>
    public async Task CheckPossessionAndSend(
        string senderFriendCode, 
        string targetFriendCode, 
        string method, 
        UserPermissions required, 
        ActionCommand request, 
        IHubCallerClients clients)
    {
        if (presence.TryGet(targetFriendCode) is not { } target)
            return;

        if (await database.GetPermissions(targetFriendCode, senderFriendCode) is not { } permissions)
            return;

        if (HasRequiredPermissions(permissions, required) is false)
            return;

        var client = clients.Client(target.ConnectionId);

        using var token = new CancellationTokenSource(TimeOutDuration);
        try
        {
            await client.SendAsync(method, request, token.Token);
        }
        catch (OperationCanceledException)
        {
            logger.LogWarning("[CheckPossessionAndInvoke] {Sender} SendAsync message timed out", senderFriendCode);
        }
        catch (Exception e)
        {
            logger.LogWarning("[CheckPossessionAndInvoke] {Sender} SendAsync message failed unexpectedly {Error}", senderFriendCode, e);
        }
    }
    
    /// <summary>
    ///     <inheritdoc cref="IForwardedRequestManager.CheckPossessionAndSend"/>
    /// </summary>
    public async Task<PossessionResponse> CheckPossessionAndInvoke(
        string senderFriendCode, 
        string targetFriendCode, 
        string method, 
        UserPermissions required, 
        ActionCommand request, 
        IHubCallerClients clients)
    {
        if (presence.TryGet(targetFriendCode) is not { } target)
            return new PossessionResponse(PossessionResponseEc.TargetOffline, PossessionResultEc.Uninitialized);
        
        if (await database.GetPermissions(targetFriendCode, senderFriendCode) is not { } permissions)
            return new PossessionResponse(PossessionResponseEc.TargetNotFriends, PossessionResultEc.Uninitialized);

        if (HasRequiredPermissions(permissions, required) is false)
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
        catch
        {
            return new PossessionResponse(PossessionResponseEc.Unknown, PossessionResultEc.Uninitialized);
        }
    }
    
    private async Task<(ISingleClientProxy client, ActionResult<Unit>? result)> EvaluateTargetAsync(string senderFriendCode, string targetFriendCode, UserPermissions required, IHubCallerClients clients)
    {
        if (presence.TryGet(targetFriendCode) is not { } target)
            return (null!, ActionResultBuilder.Fail(ActionResultEc.TargetOffline));
        
        if (await database.GetPermissions(targetFriendCode, senderFriendCode) is not { } permissions)
            return (null!, ActionResultBuilder.Fail(ActionResultEc.TargetNotFriends));

        if (HasRequiredPermissions(permissions, required) is false)
            return  (null!, ActionResultBuilder.Fail(ActionResultEc.TargetHasNotGrantedSenderPermissions));

        return (clients.Client(target.ConnectionId), null);
    }

    private static bool HasRequiredPermissions(UserPermissions granted, UserPermissions required)
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