using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using AetherRemoteCommon.Network.Enums;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Domain.Interfaces;
using AetherRemoteServer.Infrastructure.Database;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

public class BodySwapHandler(
    ILogger<BodySwapHandler> logger, 
    DatabaseInfrastructure databaseInfrastructure, 
    PresenceService presenceService, 
    RelayManager relayManager) : IRelayHandler<BodySwapPayload, BodySwapResponse>
{
    private record Character(string Name, string World);
    
    public async Task<Response<BodySwapResponse>> Execute(string senderFriendCode, Request<BodySwapPayload> request, IHubCallerClients clients)
    {
        if (ValidateBodySwapRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid body swap request {Error}", senderFriendCode, error);
            return new Response<BodySwapResponse>(error, []);
        }
        
        // Convert the swap attributes to primary permissions
        var primary = request.Payload.SwapAttributes.ToPrimaryPermissions();
        primary |= PrimaryPermissions.BodySwap;
        
        // Check elevated
        var elevated = ElevatedPermissions.None;
        if (request.Payload.LockCode is not null)
            elevated = ElevatedPermissions.PermanentTransformation;
        
        // Get the names of everyone involved in the swap
        var characters = new List<Character>();
        foreach (var targetFriendCode in request.TargetFriendCodes)
        {
            if (presenceService.TryGet(targetFriendCode) is not { } target)
                return new Response<BodySwapResponse>(ResponseStatus.TargetOffline, []);

            // Get the target's permissions for the sender
            if (await databaseInfrastructure.GetSinglePermissions(targetFriendCode, senderFriendCode) is not { } targetPermissions)
                return new Response<BodySwapResponse>(ResponseStatus.TargetNotFriends, []);

            // Get and resolve their permissions
            var global = await databaseInfrastructure.GetGlobalPermissions(targetFriendCode);
            var resolved = PermissionResolver.Resolve(global, targetPermissions);
            
            // Body swap will only every make use of primary and elevated permissions
            if ((resolved.Primary & primary) != primary || (resolved.Elevated & elevated) != elevated)
                return new Response<BodySwapResponse>(ResponseStatus.TargetHasNotGrantedPermissions, []);
            
            characters.Add(new Character(target.CharacterName, target.CharacterWorld));
        }

        // Handle the case where we want ourselves swapped too
        if (request.Payload.IncludeSelf)
        {
            if (presenceService.TryGet(senderFriendCode) is not { } sender)
            {
                logger.LogWarning("{Sender} did not have a presence", senderFriendCode);
                return new Response<BodySwapResponse>(ResponseStatus.Unknown, []);
            }
            
            characters.Add(new Character(sender.CharacterName, sender.CharacterWorld));
        }
        
        // Shuffle everyone around
        var deranged = Derange(characters);
        
        var results = new Dictionary<string, RoutedResponseStatus>();
        var pending = new Task<RoutedResponse<NoPayload>>[request.TargetFriendCodes.Count];
        for (var i = 0; i < request.TargetFriendCodes.Count; i++)
        {
            // Get the new body to be assigned to this person
            var character = deranged[i];

            // Construct the tailored request
            var payload = new BodySwapRoutedPayload(request.Payload.SwapAttributes, character.Name, character.World);
            var routed = new RoutedRequest<BodySwapRoutedPayload>(senderFriendCode, payload);

            // Double-check the target is still online
            if (presenceService.TryGet(request.TargetFriendCodes[i]) is not { } connectionClient)
            {
                pending[i] = Task.FromResult(new RoutedResponse<NoPayload>(RoutedResponseStatus.Offline));
                continue;
            }
            
            try
            {
                var client = clients.Client(connectionClient.ConnectionId);
                pending[i] = relayManager.Send<BodySwapRoutedPayload, NoPayload>(HubMethod.BodySwap, routed, client);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", senderFriendCode, request.TargetFriendCodes[i], e.Message);
                pending[i] = Task.FromResult(new RoutedResponse<NoPayload>(RoutedResponseStatus.Unknown));
            }
        }
        
        var completed = await Task.WhenAll(pending).ConfigureAwait(false);
        for (var i = 0; i < completed.Length; i++)
            results.Add(request.TargetFriendCodes[i], completed[i].Status);

        // In practice, this will never be greater than, only equal to
        if (request.TargetFriendCodes.Count >= deranged.Count)
            return new Response<BodySwapResponse>(ResponseStatus.Success, results);
        
        var own = deranged[^1];
        return new Response<BodySwapResponse>(ResponseStatus.Success, results, new BodySwapResponse(own.Name, own.World));
    }
    
    private ResponseStatus? ValidateBodySwapRequest(string senderFriendCode, Request<BodySwapPayload> request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ResponseStatus.TooManyRequests;
        
        // This function does not function if the sender includes themselves in the target
        foreach (var target in request.TargetFriendCodes)
            if (target == senderFriendCode)
                return ResponseStatus.BadRequest;
        
        // Needs at least two people total
        if (request.TargetFriendCodes.Count < 2 && request.Payload.IncludeSelf is false)
            return ResponseStatus.TooFewTargets;

        return null;
    }
    
    private static List<Character> Derange(IReadOnlyList<Character> source)
    {
        var list = source.ToList();
        var n = list.Count;

        for (var index = 0; index < n - 1; index++)
        {
            var swap = Random.Shared.Next(index + 1, n);
            (list[index], list[swap]) = (list[swap], list[index]);
        }

        if (Equals(list[n - 1], source[n - 1]) is false)
            return list;

        var fix = Random.Shared.Next(0, n - 1);
        (list[n - 1], list[fix]) = (list[fix], list[n - 1]);
        return list;
    }
}