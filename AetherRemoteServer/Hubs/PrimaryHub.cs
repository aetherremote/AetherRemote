using AetherRemoteCommon;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Commands;
using AetherRemoteServer.Authentication;
using AetherRemoteServer.Domain;
using AetherRemoteServer.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace AetherRemoteServer.Hubs;

[Authorize]
public class PrimaryHub(NetworkService network, ILogger<PrimaryHub> logger) : Hub
{
    // Injected
    private readonly NetworkService network = network;
    private readonly ILogger<PrimaryHub> logger = logger;

    /// <summary>
    /// Maps online FriendCode to ConnectionId
    /// </summary>
    public static readonly ConcurrentDictionary<string, User> ActiveUserConnections = [];

    /// <summary>
    /// Extracts FriendCode from claims
    /// </summary>
    private string FriendCode => Context?.User?.Claims.FirstOrDefault(claim => string.Equals(claim.Type, AuthClaimTypes.FriendCode, StringComparison.Ordinal))?.Value ?? throw new Exception("FriendCode not present in claims");

    [HubMethodName(Network.User.CreateOrUpdate)]
    [Authorize(Policy = "Administrator")]
    public async Task<CreateOrUpdateUserResponse> CreateOrUpdateUser(CreateOrUpdateUserRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.CreateOrUpdateUser(request);
    }

    [HubMethodName(Network.User.Delete)]
    [Authorize(Policy = "Administrator")]
    public async Task<DeleteUserResponse> DeleteUser(DeleteUserRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.DeleteUser(request);
    }

    [HubMethodName(Network.User.Get)]
    [Authorize(Policy = "Administrator")]
    public async Task<GetUserResponse> GetUser(GetUserRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.GetUser(request);
    }

    [HubMethodName(Network.LoginDetails)]
    public async Task<LoginDetailsResponse> LoginDetails(LoginDetailsRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.LoginDetails(FriendCode, request);
    }

    [HubMethodName(Network.Permissions.CreateOrUpdate)]
    public async Task<CreateOrUpdatePermissionsResponse> CreateOrUpdatePermissions(CreateOrUpdatePermissionsRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.CreateOrUpdatePermissions(FriendCode, request, Clients);
    }

    [HubMethodName(Network.Permissions.Delete)]
    public async Task<DeletePermissionsResponse> DeletePermissions(DeletePermissionsRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.DeletePermissions(FriendCode, request);
    }

    [HubMethodName(Network.Commands.BodySwap)]
    public async Task<BodySwapResponse> BodySwap(BodySwapRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.BodySwap(FriendCode, request, Clients);
    }

    [HubMethodName(Network.Commands.Emote)]
    public async Task<EmoteResponse> Emote(EmoteRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.Emote(FriendCode, request, Clients);
    }

    [HubMethodName(Network.Commands.Speak)]
    public async Task<SpeakResponse> Speak(SpeakRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.Speak(FriendCode, request, Clients);
    }

    [HubMethodName(Network.Commands.Transform)]
    public async Task<TransformResponse> Transform(TransformRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.Transform(FriendCode, request, Clients);
    }

    [HubMethodName(Network.Commands.Revert)]
    public async Task<RevertResponse> Revert(RevertRequest request)
    {
        logger.LogInformation("{Request}", request);
        return await network.Revert(FriendCode, request, Clients);
    }

    public override async Task OnConnectedAsync()
    {
        await network.UpdateOnlineStatus(FriendCode, true, Clients);
        ActiveUserConnections[FriendCode] = new User(Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await network.UpdateOnlineStatus(FriendCode, false, Clients);
        ActiveUserConnections.Remove(FriendCode, out _);
        await base.OnDisconnectedAsync(exception);
    }
}
