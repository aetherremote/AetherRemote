using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Enums.Permissions;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using AetherRemoteCommon.Util;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;
using AetherRemoteServer.Services.Database;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="BodySwapRequest"/>
/// </summary>
public class BodySwapHandler(DatabaseService database, PresenceService presenceService, ILogger<BodySwapHandler> logger)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<BodySwapResponse> Handle(string senderFriendCode, BodySwapRequest request, IHubCallerClients clients)
    {
        if (ValidateBodySwapRequest(senderFriendCode, request) is { } error)
        {
            logger.LogWarning("{Sender} sent invalid body swap request {Error}", senderFriendCode, error);
            return new BodySwapResponse(error, [], null, null);
        }
        
        // Convert the swap attributes to primary permissions
        var primary = request.SwapAttributes.ToPrimaryPermission();
        primary |= PrimaryPermissions.BodySwap;

        // Check elevated
        var elevated = ElevatedPermissions.None;
        if (request.LockCode is not null)
            elevated = ElevatedPermissions.PermanentTransformation;

        // Get the names of everyone involved in the swap
        var characters = new List<Character>();
        foreach (var targetFriendCode in request.TargetFriendCodes)
        {
            if (presenceService.TryGet(targetFriendCode) is not { } target)
                return new BodySwapResponse(ActionResponseEc.TargetOffline, [], null, null);

            // Get the target's permissions for the sender
            if (await database.GetSinglePermissions(targetFriendCode, senderFriendCode) is not { } targetPermissions)
                return new BodySwapResponse(ActionResponseEc.TargetBodySwapIsNotFriends, [], null, null);

            // Get and resolve their permissions
            var global = await database.GetGlobalPermissions(targetFriendCode);
            var resolved = PermissionResolver.Resolve(global, targetPermissions);
            
            // Body swap will only every make use of primary and elevated permissions
            if ((resolved.Primary & primary) != primary || (resolved.Elevated & elevated) != elevated)
                return new BodySwapResponse(ActionResponseEc.TargetBodySwapLacksPermissions, [], null, null);
            
            characters.Add(new Character(target.CharacterName, target.CharacterName));
        }

        // Including yourself if you marked as such
        if (request.SenderCharacterName is not null && request.SenderCharacterWorld is not null)
            characters.Add(new Character(request.SenderCharacterName, request.SenderCharacterWorld));

        // Shuffle everyone around
        var deranged = Derange(characters);

        var results = new Dictionary<string, ActionResultEc>();
        var pending = new Task<ActionResult<Unit>>[request.TargetFriendCodes.Count];
        for (var i = 0; i < request.TargetFriendCodes.Count; i++)
        {
            // Get the new body to be assigned to this person
            var character = deranged[i];
            
            // Construct the tailored request
            var forwarded = new BodySwapCommand(senderFriendCode, character.Name, character.World, request.SwapAttributes, request.LockCode);

            // Double-check the target is still online
            if (presenceService.TryGet(request.TargetFriendCodes[i]) is not { } connectionClient)
                return new BodySwapResponse(ActionResponseEc.TargetOffline, [], null, null);
            
            try
            {
                var client = clients.Client(connectionClient.ConnectionId);
                pending[i] = ForwardedRequestManager.ForwardRequestWithTimeout<Unit>(HubMethod.BodySwap, client, forwarded);
            }
            catch (Exception e)
            {
                logger.LogWarning("{Issuer} send action to {Target} failed, {Error}", senderFriendCode, request.TargetFriendCodes[i], e.Message);
                pending[i] = Task.FromResult(ActionResultBuilder.Fail(ActionResultEc.Unknown));
            }
        }

        var completed = await Task.WhenAll(pending).ConfigureAwait(false);
        for (var i = 0; i < completed.Length; i++)
            results.Add(request.TargetFriendCodes[i], completed[i].Result);

        // In practice, this will never be greater than, only equal to
        if (request.TargetFriendCodes.Count >= deranged.Count)
            return new BodySwapResponse(ActionResponseEc.Success, results, null, null);
        
        var own = deranged[^1];
        return new BodySwapResponse(ActionResponseEc.Success, results, own.Name, own.World);
    }

    private ActionResponseEc? ValidateBodySwapRequest(string senderFriendCode, BodySwapRequest request)
    {
        if (presenceService.IsUserExceedingCooldown(senderFriendCode))
            return ActionResponseEc.TooManyRequests;
        
        // This function does not function if the sender includes themselves in the target
        foreach (var target in request.TargetFriendCodes)
            if (target == senderFriendCode)
                return ActionResponseEc.IncludedSelfInBodySwap;
        
        // Needs at least two people total
        if (request.TargetFriendCodes.Count < 2 && request.SenderCharacterName is null)
            return ActionResponseEc.TooFewTargets;

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
    
    private record Character(string Name, string World);
}