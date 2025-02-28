using System;
using Dalamud.Interface;
using Dalamud.Interface.ImGuiNotification;

namespace AetherRemoteClient.Utils;

/// <summary>
///     Provides helper methods for creating common notification types
/// </summary>
public static class NotificationHelper
{
    private static readonly TimeSpan SuccessDuration = TimeSpan.FromSeconds(3);
    private static readonly TimeSpan WarningDuration = TimeSpan.FromSeconds(6);

    /// <summary>
    ///     Shorthand to create an information notification minimized
    /// </summary>
    public static Notification Info(string title, string content)
    {
        return new Notification
        {
            Type = NotificationType.Info,
            Icon = INotificationIcon.From(FontAwesomeIcon.ExclamationCircle),
            Minimized = true,
            InitialDuration = SuccessDuration,
            ExtensionDurationSinceLastInterest = TimeSpan.Zero,
            MinimizedText = title,
            Title = title,
            Content = content
        };
    }
    
    /// <summary>
    ///     Shorthand to create a success notification minimized
    /// </summary>
    public static Notification Success(string title, string content)
    {
        return new Notification
        {
            Type = NotificationType.Success,
            Icon = INotificationIcon.From(FontAwesomeIcon.CheckCircle),
            Minimized = true,
            InitialDuration = SuccessDuration,
            ExtensionDurationSinceLastInterest = TimeSpan.Zero,
            MinimizedText = title,
            Title = title,
            Content = content
        };
    }

    /// <summary>
    ///     Shorthand to create a warning notification minimized
    /// </summary>
    public static Notification Warning(string title, string content)
    {
        return new Notification
        {
            Type = NotificationType.Warning,
            Icon = INotificationIcon.From(FontAwesomeIcon.ExclamationCircle),
            Minimized = true,
            InitialDuration = WarningDuration,
            MinimizedText = title,
            Title = title,
            Content = content
        };
    }

    /// <summary>
    ///     Shorthand to create an error notification minimized
    /// </summary>
    public static Notification Error(string title, string content)
    {
        return new Notification
        {
            Type = NotificationType.Error,
            Icon = INotificationIcon.From(FontAwesomeIcon.ExclamationCircle),
            Minimized = true,
            InitialDuration = WarningDuration,
            MinimizedText = title,
            Title = title,
            Content = content
        };
    }
}