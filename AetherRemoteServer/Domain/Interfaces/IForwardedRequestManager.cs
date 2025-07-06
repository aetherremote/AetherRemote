using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Domain.Interfaces;

/// <summary>
///     Handles checking permissions and forwarding requests to clients
/// </summary>
public interface IForwardedRequestManager
{
    /// <summary>
    ///     Checks for provided permissions before attempting to send to all targets
    /// </summary>
    /// <param name="sender">Sender's friend code</param>
    /// <param name="targets">List of target friend codes</param>
    /// <param name="method">Name of the method the request will target as defined in <see cref="HubMethod"/></param>
    /// <param name="permissions">The permissions to check</param>
    /// <param name="request">The request object to send to targets</param>
    /// <param name="clients">The client context containing all the connected clients</param>
    /// <returns>The result of sending to all the clients</returns>
    public Task<ActionResponse> CheckPermissionsAndSend(string sender, List<string> targets, string method,
        UserPermissions permissions, ForwardedActionRequest request, IHubCallerClients clients);
}