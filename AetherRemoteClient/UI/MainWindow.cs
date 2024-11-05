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
    public static readonly Vector2 FriendListSize = new(150, 0);

    // Injected
    private readonly NetworkProvider _networkProvider;

    // Tabs
    private readonly DashboardTab _dashboardTab;
    private readonly FriendsTab _friendsTab;
    private readonly ControlTab _controlTab;
    private readonly HistoryTab _historyTab;
    private readonly SettingsTab _settingsTab;
    private readonly ResidualAetherTab _residualAetherTab;

    public MainWindow(
        ActionQueueProvider actionQueueProvider,
        ClientDataManager clientDataManager,
        EmoteProvider emoteProvider,
        GlamourerAccessor glamourerAccessor,
        HistoryLogManager historyLogManager,
        ModSwapManager modSwapManager,
        NetworkProvider networkProvider,
        WorldProvider worldProvider
        ) : base($"Aether Remote - Version {Plugin.Version}")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(600, 510),
            MaximumSize = ImGui.GetIO().DisplaySize
        };

        _networkProvider = networkProvider;

        _dashboardTab = new DashboardTab(clientDataManager, networkProvider);
        _friendsTab = new FriendsTab(clientDataManager, networkProvider);
        _controlTab = new ControlTab(clientDataManager, emoteProvider, glamourerAccessor, historyLogManager, modSwapManager, networkProvider, worldProvider);
        _historyTab = new HistoryTab(historyLogManager);
        _settingsTab = new SettingsTab(actionQueueProvider, clientDataManager);
        _residualAetherTab = new ResidualAetherTab(modSwapManager);
    }

    public override void Draw()
    {
        if (ImGui.BeginTabBar("AetherRemoteMainTabBar"))
        {
            _dashboardTab.Draw();
            if (_networkProvider.Connected || Plugin.DeveloperMode)
            {
                _friendsTab.Draw();
                _controlTab.Draw();
                _residualAetherTab.Draw();
                _historyTab.Draw();
            }
            _settingsTab.Draw();
            ImGui.EndTabBar();
        }
    }

    public void Dispose()
    {
        _dashboardTab.Dispose();
        _friendsTab.Dispose();
        _controlTab.Dispose();
        _historyTab.Dispose();
        _settingsTab.Dispose();
        GC.SuppressFinalize(this);
    }
}
