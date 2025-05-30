using AetherRemoteCommon.Domain.Network;
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
    public async Task<BaseResponse> Handle(string senderFriendCode, RemoveFriendRequest request)
    {
        var success = await databaseService.DeletePermissions(senderFriendCode, request.TargetFriendCode);
        return new BaseResponse { Success = success };
    }
}