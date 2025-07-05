using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network.RemoveFriend;
using AetherRemoteServer.Domain.Interfaces;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="RemoveFriendRequest"/>
/// </summary>
public class RemoveFriendHandler(IDatabaseService databaseService)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<RemoveFriendResponse> Handle(string senderFriendCode, RemoveFriendRequest request)
    {
        var result = await databaseService.DeletePermissions(senderFriendCode, request.TargetFriendCode) switch
        {
            DatabaseResultEc.NoOp => RemoveFriendEc.NotFriends,
            DatabaseResultEc.Success => RemoveFriendEc.Success,
            _ => RemoveFriendEc.Unknown
        };
        
        return new RemoveFriendResponse(result);
    }
}