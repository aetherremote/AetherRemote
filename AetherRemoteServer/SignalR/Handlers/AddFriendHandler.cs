using AetherRemoteCommon.V2.Domain.Enum;
using AetherRemoteCommon.V2.Domain.Network.AddFriend;
using AetherRemoteServer.Domain.Interfaces;

namespace AetherRemoteServer.SignalR.Handlers;

/// <summary>
///     Handles the logic for fulfilling a <see cref="AddFriendRequest"/>
/// </summary>
public class AddFriendHandler(IConnectionsService connections, IDatabaseService database)
{
    /// <summary>
    ///     Handles the request
    /// </summary>
    public async Task<AddFriendResponse> Handle(string issuerFriendCode, AddFriendRequest request)
    {
        var result = await database.CreatePermissions(issuerFriendCode, request.TargetFriendCode);
        return new AddFriendResponse
        {
            Result = result switch
            {
                DatabaseResultEc.Uninitialized => AddFriendEc.Uninitialized,
                DatabaseResultEc.NoSuchFriendCode => AddFriendEc.NoSuchFriendCode,
                DatabaseResultEc.AlreadyFriends => AddFriendEc.AlreadyFriends,
                DatabaseResultEc.Success => AddFriendEc.Success,
                _ => AddFriendEc.Unknown
            },
            Online = connections.TryGetClient(issuerFriendCode) is not null
        };
    }
}