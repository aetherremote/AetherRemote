using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.Domain.Log;

/// <summary>
/// Stores history data
/// </summary>
public class HistoryLog : AbstractHistoryLog
{
    /// <summary>
    /// <inheritdoc cref="HistoryLog"/>
    /// </summary>
    public HistoryLog(string message) : base(message) { }

    /// <summary>
    /// <inheritdoc cref="AbstractHistoryLog.Build"/>
    /// </summary>
    public override void Build()
    {
        ImGui.TextColored(ImGuiColors.TankBlue, Time.ToShortTimeString());
        ImGui.SameLine();
        ImGui.TextWrapped(Message);
    }

    /// <summary>
    /// Shorthand for generating a log when an action is blocked because sender isn't friends
    /// </summary>
    public static string NotFriends(string operationName, string sender) =>
        $"Blocked {operationName} command from {sender} who is not on your friends list";

    /// <summary>
    /// Shorthand for generating a log when an action is blocked because sender doesn't have permissions
    /// </summary>
    public static string LackingPermissions(string operationName, string sender) =>
        $"Blocked {operationName} command from {sender} who lacks permissions";

    /// <summary>
    /// Shorthand for generating a log when an action is blocked because sender sent invalid data
    /// </summary>
    public static string InvalidData(string operationName, string sender) =>
        $"Blocked {operationName} command from {sender} who sent invalid data";
}
