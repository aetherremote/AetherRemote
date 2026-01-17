using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Domain.Interfaces;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Managers;

/// <summary>
///     <inheritdoc cref="IForwardedRequestManager"/>
/// </summary>
public class ForwardedRequestManager(IDatabaseService database, IPresenceService presence, ILogger<ForwardedRequestManager> logger) : IForwardedRequestManager
{
    private static readonly TimeSpan TimeOutDuration = TimeSpan.FromSeconds(8);

    public async Task<ActionResponse> CheckPermissionsAndSend(string sender, List<string> targets, string method, UserPermissions permissions, ActionCommand request, IHubCallerClients clients)
    {
        var tasks = new Task<ActionResult<Unit>>[targets.Count];
        for (var i = 0; i < targets.Count; i++)
        {
            var target = targets[i];
            var (client, failure) = await EvaluateTargetAsync(sender, target, permissions, clients);
            
            // If there is not a failure, proceed with the call, otherwise return the failure
            tasks[i] = failure is null
                ? ForwardRequestWithTimeout<Unit>(method, client, request)
                : Task.FromResult(failure);
        }

        var completed = await Task.WhenAll(tasks);
        var results = new Dictionary<string, ActionResultEc>(targets.Count);
        for (var i = 0; i < targets.Count; i++)
            results[targets[i]] = completed[i].Result;

        return new ActionResponse(ActionResponseEc.Success, results);
    }

    private async Task<(ISingleClientProxy client, ActionResult<Unit>? result)> EvaluateTargetAsync(string senderFriendCode, string targetFriendCode, UserPermissions required, IHubCallerClients clients)
    {
        if (presence.TryGet(targetFriendCode) is not { } target)
            return (null!, ActionResultBuilder.Fail(ActionResultEc.TargetOffline));
        
        if (await database.GetPermissions(senderFriendCode, targetFriendCode) is not { } permissions)
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