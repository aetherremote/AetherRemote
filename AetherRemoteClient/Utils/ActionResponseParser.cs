using System.Collections.Generic;
using System.Text;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;

namespace AetherRemoteClient.Utils;

/// <summary>
///     Parses any response from the server and provides a notification on the screen with the results 
/// </summary>
public static class ActionResponseParser
{
    public static void Parse(string operation, ActionResponse response)
    {
        if (response.Result is not ActionResponseEc.Success)
        {
            NotificationHelper.Error($"Failed {operation} request", BuildActionFailedNotification(response.Result));
            return;
        }

        var count = 0;
        var message = new StringBuilder();
        foreach (var (target, code) in response.Results)
        {
            if (code is ActionResultEc.Success)
                continue;

            count++;
            message.AppendLine(BuildActionFailedNotification2(target, code));
        }

        if (count is 0)
        {
            NotificationHelper.Success($"{operation} - Success", string.Empty);
        }
        else if (count == response.Results.Count)
        {
            var title = $"{operation} - Failure";
            NotificationHelper.Error(title, message.ToString());
        }
        else
        {
            var title = $"{operation} - Partial Success, {count}/{response.Results.Count} failures";
            NotificationHelper.Warning(title, message.ToString());
        }
    }

    private static string BuildActionFailedNotification(ActionResponseEc code)
    {
        if (code is ActionResponseEc.Success)
            return "Code was successful but the error handling method was called.";

        return code switch
        {
            // General
            ActionResponseEc.TooManyRequests => "Too many requests, slow down!",
            ActionResponseEc.TooManyTargets =>
                "You have too many targets for this operation. In game operations are limited to 3 targets",

            ActionResponseEc.TooFewTargets => "You have too few targets selected for this operation",
            ActionResponseEc.TargetOffline => "One of your targets is offline",
            ActionResponseEc.TargetBodySwapLacksPermissions => "You lack permissions for one of your targets",
            ActionResponseEc.TargetBodySwapIsNotFriends => "You are not friends with one of your targets",

            // Disabled
            ActionResponseEc.Disabled => "This feature is temporarily disabled - How did you call this?!",
            
            // Exception
            ActionResponseEc.Unknown => "Unknown failure",

            // Default
            _ => "The error code was uninitialized."
        };
    }
    
    private static string BuildActionFailedNotification2(string target, ActionResultEc code)
    {
        if (code is ActionResultEc.Success)
            return string.Concat("No error for ", target);

        var name = Plugin.Configuration.Notes.GetValueOrDefault(target, target);
        return code switch
        {
            ActionResultEc.ClientNotFriends or
                ActionResultEc.TargetNotFriends => string.Concat("You are not friends with ", name),

            ActionResultEc.ClientInSafeMode => string.Concat(name, " is not accepting commands at the moment"),
            ActionResultEc.ClientHasFeaturePaused => string.Concat(name, " has paused this feature"),
            ActionResultEc.ClientHasSenderPaused => string.Concat(name, " did not process your command"),

            ActionResultEc.ClientHasNotGrantedSenderPermissions or
                ActionResultEc.TargetHasNotGrantedSenderPermissions => string.Concat(name,
                    " has not granted you permissions for this command"),

            ActionResultEc.ClientBadData => string.Concat(name, " could not parse the data you provided"),
            ActionResultEc.ClientPluginDependency => string.Concat(name, " ran into an issue with another plugin"),
            ActionResultEc.ClientBeingHypnotized => string.Concat(name, " is currently occupied elsewhere"),
            ActionResultEc.ClientPermanentlyTransformed => string.Concat(name, " is permanently transformed and cannot change form"),
            
            ActionResultEc.TargetTimeout => string.Concat("The command timed out to ", name),
            ActionResultEc.TargetOffline => string.Concat(name, " is offline"),
            ActionResultEc.ValueNotSet => string.Concat(name, "Report to developer the code 730"),

            _ => string.Concat(name, " encountered an unknown error")
        };
    }
}