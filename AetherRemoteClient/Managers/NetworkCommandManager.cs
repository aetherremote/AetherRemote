using System.Collections.Generic;
using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Customize;
using AetherRemoteCommon.Domain.Network.Emote;
using AetherRemoteCommon.Domain.Network.Speak;
using AetherRemoteCommon.Domain.Network.Transform;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Class responsible for sending and processing command requests to the server
/// </summary>
/// <remarks>Specifically focusing on actions a user can do to another</remarks>
public class NetworkCommandManager(CommandLockoutService commandLockoutService, NetworkService network)
{
    /// <summary>
    ///     Sends a <see cref="EmoteRequest"/> to the server
    /// </summary>
    public async Task SendEmote(List<string> targets, string emote, bool displayLogMessage)
    {
        commandLockoutService.Lock();
        var request = new EmoteRequest(targets, emote, displayLogMessage);
        var response = await network.InvokeAsync<ActionResponse>(HubMethod.Emote, request).ConfigureAwait(false);
        ActionResponseParser.Parse("Emote", response);
    }
    
    /// <summary>
    ///     Sends a <see cref="SpeakRequest"/> to the server
    /// </summary>
    public async Task SendSpeak(List<string> targets, string message, ChatChannel channel, string? extra)
    {
        commandLockoutService.Lock();
        var request = new SpeakRequest(targets, message, channel, extra);
        var response = await network.InvokeAsync<ActionResponse>(HubMethod.Speak, request).ConfigureAwait(false);

        if (channel == ChatChannel.Echo)
        {
            if (response.Result is ActionResponseEc.Success)
            {
                foreach (var (friendCode, result) in response.Results)
                {
                    if (result is not ActionResultEc.Success)
                        continue;
                    
                    Plugin.Configuration.Notes.TryGetValue(friendCode, out var note);
                    Plugin.ChatGui.Print($"Echo sent to {note ?? friendCode} >> {message}");
                }
            }
        }
        
        ActionResponseParser.Parse("Speak", response);
    }

    /// <summary>
    ///     Sends a <see cref="CustomizeRequest"/> to the server
    /// </summary>
    public async Task SendCustomize(List<string> targets, byte[] profileStringAsBytes, bool shouldApplyAsAdditive)
    {
        commandLockoutService.Lock();
        var request = new CustomizeRequest(targets, profileStringAsBytes, shouldApplyAsAdditive);
        var response = await network.InvokeAsync<ActionResponse>(HubMethod.CustomizePlus, request).ConfigureAwait(false);
        ActionResponseParser.Parse("Customize+", response);
    }

    /// <summary>
    ///     Sends a <see cref="CustomizeRequest"/> to the server
    /// </summary>
    public async Task SendTransformation(List<string> targets, string design, GlamourerApplyFlags flags)
    {
        commandLockoutService.Lock();
        var request = new TransformRequest(targets, design, flags, null);
        var response = await network.InvokeAsync<ActionResponse>(HubMethod.Transform, request).ConfigureAwait(false);
        ActionResponseParser.Parse("Transform", response);
    }
}