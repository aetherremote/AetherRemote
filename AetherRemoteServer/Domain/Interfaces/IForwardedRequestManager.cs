using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Possession;
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
    public Task<ActionResponse> CheckPermissionsAndSend(string senderFriendCode, List<string> targetFriendCodes, string method, UserPermissions required, ActionCommand request, IHubCallerClients clients);
    
    /// <summary>
    ///     TODO
    /// </summary>
    public Task CheckPossessionAndSend(string senderFriendCode, string targetFriendCode, string method, UserPermissions required, ActionCommand request, IHubCallerClients clients);
    
    /// <summary>
    ///     TODO
    /// </summary>
    public Task<PossessionResponse> CheckPossessionAndInvoke(string senderFriendCode, string targetFriendCode, string method, UserPermissions required, ActionCommand request, IHubCallerClients clients);
}