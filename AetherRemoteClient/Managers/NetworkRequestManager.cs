using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Enums;
using Dalamud.Interface.ImGuiNotification;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Class responsible for sending and processing command requests to the server
/// </summary>
/// <remarks>Specifically focusing on actions a user can do to another</remarks>
public class NetworkRequestManager(
    CommandLockoutService commandLockoutService, 
    ConfigurationService configurationService,
    NetworkService networkService)
{
    private static readonly Dictionary<ResponseStatus, string> ResponseStatusCodes = new()
    {
        [ResponseStatus.Uninitialized]                      = "Uninitialized, contact the developer",
        [ResponseStatus.Success]                            = "Success",
        [ResponseStatus.Unknown]                            = "An unknown error occurred",
        [ResponseStatus.TooFewTargets]                      = "You have too few targets selected for this operation",
        [ResponseStatus.TooManyTargets]                     = "You have too many targets selected for this operation",
        [ResponseStatus.TooManyRequests]                    = "You are making too many requests too frequently",
        [ResponseStatus.BadRequest]                         = "Your request was malformed or invalid",
        [ResponseStatus.TargetOffline]                      = "One or more of your targets is offline",
        [ResponseStatus.TargetNotFriends]                   = "One or more of your targets is not your friend",
        [ResponseStatus.TargetHasNotGrantedPermissions]     = "One of more of your targets did not grant you permissions for this operation",
        [ResponseStatus.Disabled]                           = "This operation is disabled"
    };

    private static readonly Dictionary<RoutedResponseStatus, string> RoutedResponseStatusCodes = new()
    {
        [RoutedResponseStatus.Uninitialized]                = " - Uninitialized, contact the developer",
        [RoutedResponseStatus.Success]                      = " - Success",
        [RoutedResponseStatus.Unknown]                      = " - An unknown error occurred",
        [RoutedResponseStatus.Timeout]                      = " - The request timed out",
        [RoutedResponseStatus.Offline]                      = " - is offline",
        [RoutedResponseStatus.NotFriends]                   = " - is not your friend",
        [RoutedResponseStatus.LackingPermissions]           = " - did not grant you permissions",
        [RoutedResponseStatus.SafeMode]                     = " - is not accepting commands",
        [RoutedResponseStatus.Paused]                       = " - is not accepting commands",
        [RoutedResponseStatus.BadRequest]                   = " - rejected the request",
        [RoutedResponseStatus.RuntimeError]                 = " - encountered an error while executing your request",
        [RoutedResponseStatus.BeingHypnotized]              = " - is being hypnotized"
    };
    
    /// <summary>
    ///     Wraps the provided payload in a Request, and sends it to the server. The server will then return a response payload wrapped in a Response.
    ///     When the response is returned, it will be automatically parsed and a notification will be displayed based on the results.
    /// </summary>
    /// <remarks> See <see cref="Request{TPayload}"/> and <see cref="Response{TPayload}"/> for more details. </remarks>
    public async Task<Response<TResponse>> Send<TPayload, TResponse>(List<string> targets, string method, TPayload payload)
    {
        commandLockoutService.Lock();
        var request = new Request<TPayload>(targets, payload);
        var response = await networkService.InvokeAsync<Response<TResponse>>(method, request).ConfigureAwait(true);
        ParseResponse(method, response);
        return response;
    }

    private void ParseResponse<TResponse>(string method, Response<TResponse> response)
    {
        if (response.Status is not ResponseStatus.Success)
        {
            var content = ResponseStatusCodes.GetValueOrDefault(response.Status, "Unknown");
            Notify($"{method} request failed", content, NotificationType.Error);
            return;
        }

        var failureCount = 0;
        var failureMessage = new StringBuilder();
        foreach (var (targetFriendCode, routedResponseStatus) in response.Results)
        {
            if (routedResponseStatus is RoutedResponseStatus.Success)
                continue;

            var name = configurationService.GetNoteFor(targetFriendCode) ?? targetFriendCode;
            var message = RoutedResponseStatusCodes.GetValueOrDefault(routedResponseStatus, "- Unknown");
            failureMessage.AppendLine(string.Concat(name, message));
            failureCount++;
        }

        if (failureCount is 0)
        {
            Notify($"{method} Request Succeeded", string.Empty, NotificationType.Success);
        }
        else if (failureCount == response.Results.Count)
        {
            Notify($"{method} Request Failed", failureMessage.ToString(), NotificationType.Error);
        }
        else
        {
            Notify($"{method} Request Partially Failed", failureMessage.ToString(), NotificationType.Warning);
        }
    }

    private static void Notify(string title, string content, NotificationType type)
    {
        var notification = new Notification
        {
            Minimized = type is NotificationType.Success,
            Title = title,
            MinimizedText = title,
            Content = content,
            Type = type
        };
        
        Plugin.NotificationManager.AddNotification(notification);
    }
}