using System;
using System.Collections.Generic;
using System.Text;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using Dalamud.Interface.ImGuiNotification;

namespace AetherRemoteClient.Utils;

/// <summary>
///     Parses any response from the server and provides a notification on the screen with the results 
/// </summary>
public static class ActionResponseParser
{
    private static readonly Dictionary<ActionResponseEc, string> ActionResponseErrorMessages = new()
    {
        { ActionResponseEc.Uninitialized,                           "Uninitialized action response, if you see this something is wrong" },
        { ActionResponseEc.TooManyRequests,                         "You are sending too many requests too quickly" },
        { ActionResponseEc.TooManyTargets,                          "You have too many targets selected for this action" },
        { ActionResponseEc.TooFewTargets,                           "You have too few targets selected for this action" },
        { ActionResponseEc.TargetOffline,                           "One of your targets was offline" },
        { ActionResponseEc.TargetBodySwapLacksPermissions,          "One of the targets you tried to body swap with has not granted you permissions" },
        { ActionResponseEc.TargetBodySwapIsNotFriends,              "One of the targets you tried to body swap with is not your friend" },
        { ActionResponseEc.IncludedSelfInBodySwap,                  "You may not include yourself as a target when body swapping, use the appropriate \"Include Self\" button" },
        { ActionResponseEc.BadDataInRequest,                        "Your request included invalid data" },
        { ActionResponseEc.BadTargets,                              "One of your targets does not exist" },
        { ActionResponseEc.Disabled,                                "This action is disabled" },
        { ActionResponseEc.Unknown,                                 "An unknown error has occurred" },
        { ActionResponseEc.Success,                                 "Success" }
    };

    private static readonly Dictionary<ActionResultEc, string> ActionResultErrorMessages = new()
    {
        { ActionResultEc.Uninitialized,                             "- Uninitialized action result, if you see this something is wrong" },
        { ActionResultEc.ClientNotFriends,                          "- is not one of your friends" },
        { ActionResultEc.ClientInSafeMode,                          "- is not accepting commands at the moment" },
        { ActionResultEc.ClientHasFeaturePaused,                    "- is not accepting this action at the moment" },
        { ActionResultEc.ClientHasSenderPaused,                     "- is not accepting commands at the moment" },
        { ActionResultEc.ClientHasNotGrantedSenderPermissions,      "- has not granted you permissions" },
        { ActionResultEc.ClientBadData,                             "- could not process the command" },
        { ActionResultEc.ClientPluginDependency,                    "- encountered an error with another plugin" },
        { ActionResultEc.ClientBeingHypnotized,                     "- is occupied at the moment" },
        { ActionResultEc.ClientPermanentlyTransformed,              "- is stuck in their current appearance" },
        { ActionResultEc.TargetOffline,                             "- is offline" },
        { ActionResultEc.TargetNotFriends,                          "- is not your friend" },
        { ActionResultEc.TargetHasNotGrantedSenderPermissions,      "- has not granted you permissions" },
        { ActionResultEc.TargetTimeout,                             "- had the request timeout" },
        { ActionResultEc.HasNotAcceptedAgreement,                   "- has not accepted an agreement for this action" },
        { ActionResultEc.Success,                                   "- Success" },
        { ActionResultEc.ValueNotSet,                               "- had a value not set (this should never happen, contact a developer)" },
        { ActionResultEc.Unknown,                                   "- An unknown error has occurred" }
    };

    public static void SanityCheck()
    {
        if (Enum.GetValues<ActionResponseEc>().Length != ActionResponseErrorMessages.Count)
        {
            var notification = new Notification
            {
                Title = "ActionResponseEc Case Not Covered",
                MinimizedText = "ActionResponseEc Case Not Covered",
                Type = NotificationType.Warning
            };

            Plugin.NotificationManager.AddNotification(notification);
        }

        if (Enum.GetValues<ActionResultEc>().Length != ActionResultErrorMessages.Count)
        {
            var notification = new Notification
            {
                Title = "ActionResultEc Case Not Covered",
                MinimizedText = "ActionResultEc Case Not Covered",
                Type = NotificationType.Warning
            };

            Plugin.NotificationManager.AddNotification(notification);
        }
    }

    /// <summary>
    ///     Parses the <see cref="ActionResponse"/> and displays a Dalamud notification with the success result
    /// </summary>
    public static void Parse(string operation, ActionResponse response)
    {
        if (response.Result is not ActionResponseEc.Success)
        {
            var title = string.Concat(operation, " request failed");
            var success = new Notification
            {
                Minimized = false,
                MinimizedText = title,
                Title = title,
                Content = ActionResponseErrorMessages.TryGetValue(response.Result, out var errorMessage) ? errorMessage : string.Empty,
                Type = NotificationType.Error
            };

            Plugin.NotificationManager.AddNotification(success);
        }

        var failureCount = 0;
        var failureMessage = new StringBuilder();
        foreach (var (target, code) in response.Results)
        {
            if (code is ActionResultEc.Success)
                continue;
            
            var note = Plugin.Configuration.Notes.GetValueOrDefault(target, target);
            var message = ActionResultErrorMessages.GetValueOrDefault(code, "Unknown ActionResultEc");
            failureMessage.AppendLine(string.Concat(note, message));
            failureCount++;
        }

        // Notification we will push
        Notification notification;

        // If 0 targets failed to execute the command
        if (failureCount is 0)
        {
            var title = string.Concat(operation, " request succeeded");
            notification = new Notification
            {
                Minimized = true,
                MinimizedText = title,
                Title = title,
                Content = string.Empty,
                Type = NotificationType.Success
            };
        }
        // If all targets failed to execute the command
        else if (failureCount == response.Results.Count)
        {
            var title = string.Concat(operation, " request failed");
            notification = new Notification
            {
                Minimized = false,
                MinimizedText = title,
                Title = title,
                Content = failureMessage.ToString(),
                Type = NotificationType.Error
            };
        }
        // If some targets succeeded and some didn't
        else
        {
            var title = string.Concat(operation, " partially succeeded");
            notification = new Notification
            {
                Minimized = false,
                MinimizedText = title,
                Title = title,
                Content = failureMessage.ToString(),
                Type = NotificationType.Warning
            };
        }
        
        // Actually push the notification
        Plugin.NotificationManager.AddNotification(notification);
    }
}