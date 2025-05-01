using AetherRemoteCommon.Domain.Network;
using AetherRemoteServer.Managers;
using AetherRemoteServer.Services;

namespace AetherRemoteServer.Hubs.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="AddFriendRequest"/>
/// </summary>
public class AddFriendHandler(DatabaseService databaseService, ConnectedClientsManager connectedClientsManager)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<AddFriendResponse> Handle(string issuerFriendCode, AddFriendRequest request)
    {
        var success = await databaseService.CreatePermissions(issuerFriendCode, request.TargetFriendCode);
        return new AddFriendResponse
        {
            Success = success,
            Online = connectedClientsManager.ConnectedClients.ContainsKey(issuerFriendCode)
        };
    }
}