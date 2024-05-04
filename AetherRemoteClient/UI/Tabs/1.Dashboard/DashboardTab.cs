using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Translators;
using AetherRemoteClient.Providers;
using AetherRemoteCommon.Domain;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System.Numerics;
namespace AetherRemoteClient.UI.Tabs.Dashboard;

public class DashboardTab : ITab
{
    private readonly Configuration configuration;
    private readonly FriendListProvider friendListProvider;
    private readonly NetworkProvider networkProvider;
    private readonly SecretProvider secretProvider;
    private readonly IPluginLog logger;
    
    private static readonly int LoginElementsWidth = 200;
    private static readonly int LoginButtonWidth = 50;

    private string secretInputText;

    public DashboardTab(Configuration configuration, FriendListProvider friendListProvider, NetworkProvider networkProvider, SecretProvider secretProvider, IPluginLog logger)
    {
        this.configuration = configuration;
        this.friendListProvider = friendListProvider;
        this.networkProvider = networkProvider;
        this.secretProvider = secretProvider;
        this.logger = logger;

        secretInputText = secretProvider.Secret;

        if (configuration.AutoConnect)
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

        var friendCode = Plugin.DeveloperMode ? "Dev Mode" : networkProvider.FriendCode ?? "null";
        var friendCodeSize = ImGui.CalcTextSize(friendCode);
        
        ImGui.SetCursorPosX((width / 2) - (friendCodeSize.X / 2));
        ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.ParsedOrange);
        if (ImGui.Selectable(friendCode, false, ImGuiSelectableFlags.None, friendCodeSize))
            ImGui.SetClipboardText(friendCode);

        ImGui.PopStyleColor();

        SharedUserInterfaces.Tooltip("Copy to Clipboard");
        SharedUserInterfaces.PopBigFont();
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
        if (ImGui.Checkbox("Auto Sign In", ref configuration.AutoConnect))
            configuration.Save();

        ImGui.SameLine();

        ImGui.SetCursorPosX(x + LoginElementsWidth - LoginButtonWidth);
        if (ImGui.Button("Login", new Vector2(LoginButtonWidth, 0)))
            shouldLogin = true;

        if (shouldLogin)
        {
            secretProvider.Secret = secretInputText;
            secretProvider.Save();
            Login();
        }

        if (state == ServerConnectionState.Connecting || state == ServerConnectionState.Reconnecting)
            ImGui.EndDisabled();
    }

    private async void Login()
    {
        var connectResult = await networkProvider.Connect(secretProvider.Secret);
        if (connectResult.Success == false)
            return;

        var commonFriendList = FriendTranslator.DomainFriendListToCommon(friendListProvider.FriendList);
        var hash = await AetherRemoteHash.ComputeFriendListHash(commonFriendList);
        await networkProvider.Sync(secretInputText, hash);
    }
}
