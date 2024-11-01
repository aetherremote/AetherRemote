using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI.Tabs.Control;
using AetherRemoteClient.UI.Tabs.Dashboard;
using AetherRemoteClient.UI.Tabs.Friends;
using AetherRemoteClient.UI.Tabs.History;
using AetherRemoteClient.UI.Tabs.ResidualAether;
using AetherRemoteClient.UI.Tabs.Settings;
using Dalamud.Interface.Colors;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using System;
using System.Numerics;

namespace AetherRemoteClient.UI;

public class MainWindow : Window, IDisposable
{
    // Const
    private const ImGuiWindowFlags MainWindowFlags = ImGuiWindowFlags.None;
    public static readonly Vector2 FriendListSize = new(150, 0);

    // Injected
    private readonly NetworkProvider networkProvider;

    // Tabs
    private readonly DashboardTab dashboardTab;
    private readonly FriendsTab friendsTab;
    private readonly ControlTab controlTab;
    private readonly HistoryTab historyTab;
    private readonly SettingsTab settingsTab;
    private readonly ResidualAetherTab residualAetherTab;

    public MainWindow(
        ActionQueueProvider actionQueueProvider,
        ClientDataManager clientDataManager,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager,
        ModSwapManager modSwapManager,
        NetworkProvider networkProvider,
        WorldProvider worldProvider
        ) : base($"Aether Remote - Version {Plugin.Version}", MainWindowFlags)
    {
        SizeConstraints = new WindowSizeConstraints()
        {
            MinimumSize = new Vector2(600, 510),
            MaximumSize = ImGui.GetIO().DisplaySize,
        };

        this.networkProvider = networkProvider;

        dashboardTab = new DashboardTab(clientDataManager, networkProvider);
        friendsTab = new FriendsTab(clientDataManager, networkProvider);
        controlTab = new ControlTab(clientDataManager, emoteProvider, glamourerAccessor, historyLogManager, modSwapManager, networkProvider, worldProvider);
        historyTab = new HistoryTab(historyLogManager);
        settingsTab = new SettingsTab(actionQueueProvider, clientDataManager);
        residualAetherTab = new ResidualAetherTab(modSwapManager);
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("AetherRemoteMainTabBar"))
        {
            dashboardTab.Draw();
            if (networkProvider.Connected || Plugin.DeveloperMode)
            {
                friendsTab.Draw();
                controlTab.Draw();
                residualAetherTab.Draw();
                historyTab.Draw();
            }
            settingsTab.Draw();
            ImGui.EndTabBar();
        }
    }

    public void Dispose()
    {
        dashboardTab.Dispose();
        friendsTab.Dispose();
        controlTab.Dispose();
        historyTab.Dispose();
        settingsTab.Dispose();
        GC.SuppressFinalize(this);
    }
}
