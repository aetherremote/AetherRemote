using AetherRemoteCommon.Domain;
using AetherRemoteCommon.V2;
using AetherRemoteCommon.V2.Domain;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Managers;

public class RelayManager(PermissionsService permissionsService, PresenceService presenceService)
{
    private readonly TimeSpan _timeOutDuration = TimeSpan.FromSeconds(8);
    
    /// <summary>
    ///     Simple relayer, taking the content of a request and forwarding
    /// </summary>
    public async Task<Dictionary<string, RoutedResponseStatus>> Relay<TPayload, TResponse>(
        string senderFriendCode,
        string hubMethod,
        Request<TPayload> request,
        ResolvedPermissions required,
        IHubCallerClients clients)
    {
        var routed = new RoutedRequest<TPayload>(senderFriendCode, request.Payload);
        return await Relay<TPayload, TResponse>(hubMethod, senderFriendCode, request.TargetFriendCodes, _ => routed, required, clients);
    }

    /// <summary>
    ///     Complex relayer, sending specific payloads to specific targets
    /// </summary>
    public async Task<Dictionary<string, RoutedResponseStatus>> Relay<TPayload, TResponse>(
        string hubMethod,
        string senderFriendCode,
        string[] targetFriendCodes,
        RoutedRequest<TPayload>[] requests,
        ResolvedPermissions required,
        IHubCallerClients clients)
    {
        return await Relay<TPayload, TResponse>(hubMethod, senderFriendCode, targetFriendCodes, i => requests[i], required, clients);
    }

    private async Task<Dictionary<string, RoutedResponseStatus>> Relay<TPayload, TResponse>(
        string hubMethod,
        string senderFriendCode,
        IReadOnlyList<string> targetFriendCodes,
        Func<int, RoutedRequest<TPayload>> routedPayload,
        ResolvedPermissions required,
        IHubCallerClients clients)
    {
        var count = targetFriendCodes.Count;
        var tasks = new Task<RoutedResponse<TResponse>>[count];
        for (var i = 0; i < count; i++)
        {
            if (await permissionsService.ValidatePermissions(senderFriendCode, targetFriendCodes[i], required) is { } error)
            {
                tasks[i] = Task.FromResult(new RoutedResponse<TResponse>(error));
                continue;
            }
            
            if (presenceService.TryGet(targetFriendCodes[i]) is not { } presence)
            {
                tasks[i] = Task.FromResult(new RoutedResponse<TResponse>(RoutedResponseStatus.NotOnline));
                continue;
            }
            
            tasks[i] = Send<TPayload, TResponse>(hubMethod, routedPayload(i), clients.Client(presence.ConnectionId));
        }
        
        var completed = await Task.WhenAll(tasks);
        var results = new Dictionary<string, RoutedResponseStatus>(count);
        for (var i = 0; i < count; i++)
            results[targetFriendCodes[i]] = completed[i].Status;
        
        return results;
    }

    private async Task<RoutedResponse<TResponse>> Send<TPayload, TResponse>(string method, RoutedRequest<TPayload> routed, ISingleClientProxy client)
    {
        using var token = new CancellationTokenSource(_timeOutDuration);

        try
        {
            return await client.InvokeAsync<RoutedResponse<TResponse>>(method, routed, token.Token);
        }
        catch (OperationCanceledException)
        {
            return new RoutedResponse<TResponse>(RoutedResponseStatus.Timeout);
        }
        catch
        {
            return new RoutedResponse<TResponse>(RoutedResponseStatus.Unknown);
        }
    }
}