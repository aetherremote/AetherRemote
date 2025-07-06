using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.BodySwap;
using Microsoft.AspNetCore.SignalR;

namespace AetherRemoteServer.Domain.Interfaces;

/// <summary>
///     TODO
/// </summary>
public interface IForwardedRequestManager
{
    /// <summary>
    ///     TODO
    /// </summary>
    /// <param name="senderFriendCode"></param>
    /// <param name="targetFriendCodes"></param>
    /// <param name="requestInfo"></param>
    /// <param name="clients"></param>
    /// <returns></returns>
    public Task<ActionResponse> Send(string senderFriendCode, List<string> targetFriendCodes,
        PrimaryRequestInfo requestInfo, IHubCallerClients clients);
    
    /// <summary>
    ///     TODO
    /// </summary>
    /// <param name="senderFriendCode"></param>
    /// <param name="targetFriendCodes"></param>
    /// <param name="requestInfo"></param>
    /// <param name="clients"></param>
    /// <returns></returns>
    public Task<ActionResponse> Send(string senderFriendCode, List<string> targetFriendCodes,
        SpeakRequestInfo requestInfo, IHubCallerClients clients);
    
    /// <summary>
    ///     TODO
    /// </summary>
    /// <param name="senderFriendCode"></param>
    /// <param name="targetFriendCodes"></param>
    /// <param name="characterNames"></param>
    /// <param name="attributes"></param>
    /// <param name="clients"></param>
    /// <returns></returns>
    public Task<BodySwapResponse> SendBodySwap(string senderFriendCode, List<string> targetFriendCodes,
        List<string> characterNames, CharacterAttributes attributes, IHubCallerClients clients);
}