using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Logger;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Control;
using AetherRemoteClient.UI.Tabs.Dashboard;
using AetherRemoteClient.UI.Tabs.Friends;
using AetherRemoteClient.UI.Tabs.Logs;
using AetherRemoteClient.UI.Tabs.Settings;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI;

public class MainWindow : Window, IDisposable
{
    // Constants
    private const ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.None;

    // Statics
    public static readonly Vector2 FriendListSize = new(150, 0);

    // Injected
    private readonly NetworkProvider networkProvider;

    // Tabs
    private readonly DashboardTab dashboardTab;
    private readonly FriendsTab friendsTab;
    private readonly LogsTab logsTab;
    private readonly SettingsTab settingsTab;
    private readonly ControlTab controlTab;

    public MainWindow(
        AetherRemoteLogger logger,
        ClientDataManager clientDataManager,
        Configuration configuration,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        NetworkProvider networkProvider,
        IClientState clientState,
        ITargetManager targetManager
        ) : base($"Aether Remote - Version {Plugin.Version}", MainWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(600, 500),
            MaximumSize = ImGui.GetIO().DisplaySize,
        };

        this.networkProvider = networkProvider;

        dashboardTab = new DashboardTab(logger, clientDataManager, configuration, networkProvider);
        friendsTab = new FriendsTab(logger, clientDataManager, configuration, networkProvider);
        logsTab = new LogsTab(logger);
        settingsTab = new SettingsTab(configuration);
        controlTab = new ControlTab(logger, clientDataManager, configuration, emoteProvider, 
            glamourerAccessor, networkProvider,  clientState, targetManager);
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("AetherRemoteMainTabBar"))
        {
            dashboardTab.Draw();
            if (Plugin.DeveloperMode || networkProvider.ConnectionState == ServerConnectionState.Connected)
            {
                friendsTab.Draw();
                controlTab.Draw();
                logsTab.Draw();
            }
            settingsTab.Draw();

            ImGui.EndTabBar();
        }
    }

    public void Dispose()
    {
        dashboardTab.Dispose();
        friendsTab.Dispose();
        logsTab.Dispose();
        settingsTab.Dispose();
        controlTab.Dispose();
        GC.SuppressFinalize(this);
    }
}
