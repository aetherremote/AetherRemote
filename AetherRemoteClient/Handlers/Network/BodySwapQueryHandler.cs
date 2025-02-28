using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Util;

namespace AetherRemoteClient.Handlers.Network;

/// <summary>
///     Handles a <see cref="BodySwapQueryRequest"/>
/// </summary>
public class BodySwapQueryHandler(
    FriendsListService friendsListService,
    IdentityService identityService,
    OverrideService overrideService,
    LogService logService)
{
    /// <summary>
    ///     <inheritdoc cref="BodySwapQueryHandler"/>
    /// </summary>
    public async Task<BodySwapQueryResponse> Handle(BodySwapQueryRequest request)
    {
        // Not friends
        if (friendsListService.Get(request.SenderFriendCode) is not { } friend)
        {
            logService.NotFriends("Body Swap Query", request.SenderFriendCode);
            return new BodySwapQueryResponse();
        }

        // Plugin in safe mode
        if (Plugin.Configuration.SafeMode)
        {
            logService.SafeMode("Body Swap Query", friend.NoteOrFriendCode);
            return new BodySwapQueryResponse();
        }

        // Overriding body swaps
        if (overrideService.HasActiveOverride(PrimaryPermissions.BodySwap))
        {
            logService.Override("Body Swap Query", friend.NoteOrFriendCode);
            return new BodySwapQueryResponse();
        }
        
        // Lacking permissions for body swap
        if (friend.PermissionsGrantedToFriend.Has(PrimaryPermissions.BodySwap) is false)
        {
            logService.LackingPermissions("Body Swap Query", friend.NoteOrFriendCode);
            return new BodySwapQueryResponse();
        }

        // Check if local body is present
        if (await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer).ConfigureAwait(false) is not { } player)
        {
            logService.MissingLocalBody("Body Swap", friend.NoteOrFriendCode);
            return new BodySwapQueryResponse();
        }
        
        return new BodySwapQueryResponse
        {
            Identity = new CharacterIdentity
            {
                GameObjectName = player.Name.TextValue,
                CharacterName = identityService.Identity
            }
        };
    }
}