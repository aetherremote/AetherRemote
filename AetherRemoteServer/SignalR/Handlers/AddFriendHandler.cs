using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Domain.Interfaces;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="AddFriendRequest"/>
/// </summary>
public class AddFriendHandler(IClientConnectionService connections, IDatabaseService database)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<AddFriendResponse> Handle(string issuerFriendCode, AddFriendRequest request)
    {
        var success = await database.CreatePermissions(issuerFriendCode, request.TargetFriendCode);
        return new AddFriendResponse
        {
            Success = success,
            Online = connections.TryGetClient(issuerFriendCode) is not null
        };
    }
}