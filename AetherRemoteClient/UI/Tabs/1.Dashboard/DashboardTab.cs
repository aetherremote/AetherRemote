using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Logger;
using AetherRemoteClient.Providers;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Dashboard;

public class DashboardTab : ITab
{
    private readonly Configuration configuration;
    private readonly NetworkProvider networkProvider;
    private readonly AetherRemoteLogger logger;
    private readonly ClientDataManager clientDataManager;
    
    private static readonly int LoginElementsWidth = 200;
    private static readonly int LoginButtonWidth = 50;

    private string secretInputText;

    public DashboardTab(Configuration configuration, NetworkProvider networkProvider, AetherRemoteLogger logger, ClientDataManager clientDataManager)
    {
        this.configuration = configuration;
        this.networkProvider = networkProvider;
        this.logger = logger;
        this.clientDataManager = clientDataManager;

        secretInputText = configuration.Secret;

        if (configuration.AutoLogin)
            Login();
    }

    public void Draw()
    {
        if (ImGui.BeginTabItem("Dashboard"))
        {
            if (ImGui.BeginChild("DashboardArea", Vector2.Zero, true))
            {
                if (Plugin.DeveloperMode)
                {
                    if (ImGui.Button("Toggle"))
                    {
                        if (networkProvider.ConnectionState == ServerConnectionState.Connected)
                            networkProvider.ConnectionState = ServerConnectionState.Disconnected;
                        else
                            networkProvider.ConnectionState = ServerConnectionState.Connected;
                    }

                    ImGui.SameLine();
                }

                var state = networkProvider.ConnectionState;
                var color = state switch
                {
                    ServerConnectionState.Connected => ImGuiColors.ParsedGreen,
                    ServerConnectionState.Disconnected => ImGuiColors.DPSRed,
                    _ => ImGuiColors.DalamudYellow,
                };

                if (state == ServerConnectionState.Connected)
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
    }

    private void DrawConnectedMenu()
    {
        SharedUserInterfaces.PushBigFont();

        var width = ImGui.GetWindowWidth();
        var fontSize = ImGui.GetFontSize();
        var windowPadding = ImGui.GetStyle().WindowPadding;
        
        ImGui.SetCursorPos(new Vector2(width - fontSize - windowPadding.X, windowPadding.Y));
        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Plug, new Vector2(fontSize, fontSize)))
            networkProvider.Disconnect();

        SharedUserInterfaces.PopBigFont();
        SharedUserInterfaces.Tooltip("Disconnect");
        SharedUserInterfaces.PushBigFont();

        ImGui.SetCursorPosY(windowPadding.Y);
        SharedUserInterfaces.TextCentered("My Friend Code");

        var friendCode = clientDataManager.FriendCode;
        var friendCodeSize = ImGui.CalcTextSize(friendCode);
        
        ImGui.SetCursorPosX((width / 2) - (friendCodeSize.X / 2));
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedOrange);
        if (ImGui.Selectable(friendCode, false, ImGuiSelectableFlags.None, friendCodeSize))
            ImGui.SetClipboardText(friendCode);

        ImGui.PopStyleColor();

        SharedUserInterfaces.PopBigFont();
        SharedUserInterfaces.Tooltip("Copy to Clipboard");
    }

    private void DrawDisconnectedMenu(ServerConnectionState state)
    {
        if (state == ServerConnectionState.Connecting || state == ServerConnectionState.Reconnecting)
            ImGui.BeginDisabled();

        var shouldLogin = false;
        var width = ImGui.GetWindowWidth();
        var height = ImGui.GetWindowHeight();

        SharedUserInterfaces.BigTextCentered("Aether Remote", ImGuiColors.ParsedOrange);

        ImGui.SetCursorPosY((height / 2) - (ImGui.GetFontSize() * 2));
        
        var x = (width / 2) - (LoginElementsWidth / 2);
        ImGui.SetCursorPosX(x);
        ImGui.SetNextItemWidth(LoginElementsWidth);
        if (ImGui.InputTextWithHint("##Login", "Secret", ref secretInputText, 60, ImGuiInputTextFlags.EnterReturnsTrue))
            shouldLogin = true;

        ImGui.SetCursorPosX(x);
        if (ImGui.Checkbox("Auto Login", ref configuration.AutoLogin))
            configuration.Save();

        ImGui.SameLine();

        ImGui.SetCursorPosX(x + LoginElementsWidth - LoginButtonWidth);
        if (ImGui.Button("Login", new Vector2(LoginButtonWidth, 0)))
            shouldLogin = true;

        if (shouldLogin)
        {
            configuration.Secret = secretInputText;
            configuration.Save();
            Login();
        }

        if (state == ServerConnectionState.Connecting || state == ServerConnectionState.Reconnecting)
            ImGui.EndDisabled();
    }

    private async void Login()
    {
        var connectResult = await networkProvider.Connect(configuration.Secret);
        if (connectResult.Success == false)
            return;
    }

    public void Dispose() { GC.SuppressFinalize(this); }
}
