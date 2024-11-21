using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Dashboard;

public class DashboardTab : ITab
{
    // Const
    private const int LoginElementsWidth = 200;
    private static readonly Vector2 LoginButtonSize = new(50, 0);

    // Injected
    private readonly ClientDataManager _clientDataManager;
    private readonly NetworkProvider _networkProvider;

    private string _secretInputText;

    public DashboardTab(ClientDataManager clientDataManager, NetworkProvider networkProvider)
    {
        _clientDataManager = clientDataManager;
        _networkProvider = networkProvider;

        _secretInputText = Plugin.Configuration.Secret;

        if (Plugin.Configuration.AutoLogin)
            Login();
    }

    public void Draw()
    {
        if (ImGui.BeginTabItem("Dashboard") == false) return;
        if (ImGui.BeginChild("DashboardArea", Vector2.Zero, true))
        {
            var state = _networkProvider.State;
            var color = _networkProvider.Connected
                ? ImGuiColors.ParsedGreen
                : state == HubConnectionState.Disconnected ? ImGuiColors.DPSRed : ImGuiColors.DalamudYellow;

            if (state is HubConnectionState.Connected)
                DrawConnectedMenu();
            else
                DrawDisconnectedMenu(state);

            // Connection Status
            ImGui.SetCursorPosY(ImGui.GetWindowHeight() - ImGui.GetStyle().WindowPadding.Y - ImGui.GetFontSize());
            ImGui.Text("Server Status:");
            ImGui.SameLine();
            ImGui.TextColored(color, state.ToString());

            // Plugin Version
            ImGui.SameLine();
            var version = $"{Plugin.Stage} {Plugin.Version}";
            var versionWidth = ImGui.CalcTextSize(version).X;
            ImGui.SetCursorPosX(ImGui.GetWindowWidth() - versionWidth - ImGui.GetStyle().WindowPadding.X);
            ImGui.Text(version);

            ImGui.EndChild();
        }

        ImGui.EndTabItem();
    }

    private void DrawConnectedMenu()
    {
        SharedUserInterfaces.PushBigFont();

        var width = ImGui.GetWindowWidth();
        var fontSize = ImGui.GetFontSize();
        var windowPadding = ImGui.GetStyle().WindowPadding;

        ImGui.SetCursorPos(new Vector2(width - fontSize - windowPadding.X, windowPadding.Y));
        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plug, new Vector2(fontSize, fontSize)))
            _ = _networkProvider.Disconnect();

        SharedUserInterfaces.PopBigFont();
        SharedUserInterfaces.Tooltip("Disconnect");
        SharedUserInterfaces.PushBigFont();

        ImGui.SetCursorPosY(windowPadding.Y);
        SharedUserInterfaces.TextCentered("My Friend Code");

        var friendCode = _clientDataManager.FriendCode ?? "Fetching...";
        var friendCodeSize = ImGui.CalcTextSize(friendCode);

        ImGui.SetCursorPosX(width / 2 - friendCodeSize.X / 2);
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedOrange);

        if (ImGui.Selectable(friendCode, false, ImGuiSelectableFlags.None, friendCodeSize))
            ImGui.SetClipboardText(friendCode);

        ImGui.PopStyleColor();

        SharedUserInterfaces.PopBigFont();
        SharedUserInterfaces.Tooltip("Copy to Clipboard");
    }

    private void DrawDisconnectedMenu(HubConnectionState state)
    {
        var isConnecting = state is HubConnectionState.Connecting or HubConnectionState.Reconnecting;
        SharedUserInterfaces.DisableIf(isConnecting, () =>
        {
            var shouldLogin = false;
            var width = ImGui.GetWindowWidth();
            var height = ImGui.GetWindowHeight();

            SharedUserInterfaces.BigTextCentered("Aether Remote", ImGuiColors.ParsedOrange);

            ImGui.SetCursorPosY(height / 2 - ImGui.GetFontSize() * 2);

            var centerTextX = width / 2 - LoginElementsWidth / 2.0f;
            ImGui.SetCursorPosX(centerTextX);
            ImGui.SetNextItemWidth(LoginElementsWidth);
            if (ImGui.InputTextWithHint("##Login", "Secret", ref _secretInputText, 60, ImGuiInputTextFlags.EnterReturnsTrue))
                shouldLogin = true;

            ImGui.SetCursorPosX(centerTextX);
            if (ImGui.Checkbox("Auto Login", ref Plugin.Configuration.AutoLogin))
                Plugin.Configuration.Save();

            ImGui.SameLine();

            ImGui.SetCursorPosX(centerTextX + LoginElementsWidth - LoginButtonSize.X);
            if (ImGui.Button("Login", LoginButtonSize))
                shouldLogin = true;

            if (shouldLogin == false) return;
            Plugin.Configuration.Secret = _secretInputText;
            Plugin.Configuration.Save();
            Login();
        });
    }

    private void Login()
    {
        // Don't auto login if secret is empty
        if (string.IsNullOrEmpty(Plugin.Configuration.Secret))
            return;

        _ = _networkProvider.Connect();
    }

    public void Dispose() => GC.SuppressFinalize(this);
}
