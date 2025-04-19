using System;
using System.Numerics;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.UI.Views.BodySwap;
using AetherRemoteClient.UI.Views.CustomizePlus;
using AetherRemoteClient.UI.Views.Emote;
using AetherRemoteClient.UI.Views.Friends;
using AetherRemoteClient.UI.Views.History;
using AetherRemoteClient.UI.Views.Hypnosis;
using AetherRemoteClient.UI.Views.Login;
using AetherRemoteClient.UI.Views.Moodles;
using AetherRemoteClient.UI.Views.Overrides;
using AetherRemoteClient.UI.Views.Settings;
using AetherRemoteClient.UI.Views.Speak;
using AetherRemoteClient.UI.Views.Status;
using AetherRemoteClient.UI.Views.Transformation;
using AetherRemoteClient.UI.Views.Twinning;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Windowing;
using ImGuiNET;
using Microsoft.AspNetCore.SignalR.Client;

namespace AetherRemoteClient.UI;

public class MainWindow : Window, IDisposable
{
    // Const
    private static readonly Vector2 AlignButtonTextLeft = new(0, 0.5f);
    private static readonly Vector2 NavBarDimensions = new(180, 0);
    private static readonly string MainWindowTitle = $"Aether Remote 2 - Version {Plugin.Version}";

    // Injected
    private readonly FriendsListService _friendsListService;
    private readonly NetworkService _networkService;

    // Components
    private readonly FriendsListComponentUi _friendsListComponent;

    // Views
    private readonly BodySwapViewUi _bodySwapView;
    private readonly CustomizePlusViewUi _customizePlusView;
    private readonly EmoteViewUi _emoteView;
    private readonly FriendsViewUi _friendsView;
    private readonly HistoryViewUi _historyView;
    private readonly HypnosisViewUi _hypnosisView;
    private readonly LoginViewUi _loginView;
    private readonly MoodlesViewUi _moodlesView;
    private readonly OverridesViewUi _overridesView;
    private readonly SettingsViewUi _settingsView;
    private readonly SpeakViewUi _speakView;
    private readonly StatusViewUi _statusView;
    private readonly TransformationViewUi _transformationView;
    private readonly TwinningViewUi _twinningView;
    private IDrawable _currentView;

    public MainWindow(
        CommandLockoutService commandLockoutService,
        EmoteService emoteService,
        FriendsListService friendsListService,
        IdentityService identityService,
        LogService logService,
        NetworkService networkService,
        OverrideService overrideService,
        SpiralService spiralService,
        TipService tipService,
        WorldService worldService,
        CustomizePlusIpc customize,
        GlamourerIpc glamourer,
        MoodlesIpc moodles,
        PenumbraIpc penumbra,
        ActionQueueManager actionQueueManager,
        ModManager modManager) : base(MainWindowTitle)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 500),
            MaximumSize = ImGui.GetIO().DisplaySize
        };

        // Components
        _friendsListComponent = new FriendsListComponentUi(friendsListService, networkService);

        // Views
        _statusView = new StatusViewUi(networkService, identityService, tipService, spiralService, glamourer);
        _friendsView = new FriendsViewUi(friendsListService, networkService);
        _overridesView = new OverridesViewUi(overrideService);
        _speakView = new SpeakViewUi(commandLockoutService, friendsListService, networkService, worldService);
        _emoteView = new EmoteViewUi(commandLockoutService, emoteService, friendsListService, networkService);
        _transformationView =
            new TransformationViewUi(commandLockoutService, friendsListService, networkService, glamourer);
        _bodySwapView = new BodySwapViewUi(commandLockoutService, identityService, friendsListService, networkService,
            modManager);
        _twinningView = new TwinningViewUi(commandLockoutService, friendsListService, identityService, networkService);
        _historyView = new HistoryViewUi(logService);
        _hypnosisView = new HypnosisViewUi(friendsListService, networkService, spiralService);
        _settingsView = new SettingsViewUi(spiralService, customize, glamourer, moodles, penumbra, actionQueueManager);
        _loginView = new LoginViewUi(networkService);
        _moodlesView = new MoodlesViewUi(commandLockoutService, friendsListService, networkService);
        _customizePlusView = new CustomizePlusViewUi(commandLockoutService, friendsListService, networkService);

        _friendsListService = friendsListService;
        _networkService = networkService;
        _networkService.Connected += OnConnected;
        _networkService.Connection.Reconnected += OnReconnected;
        _networkService.Connection.Reconnecting += OnReconnecting;
        _networkService.Connection.Closed += OnClosed;

        _currentView = _loginView;
    }

    public void Dispose()
    {
        _networkService.Connected -= OnConnected;
        _networkService.Connection.Reconnected -= OnReconnected;
        _networkService.Connection.Reconnecting -= OnReconnecting;
        _networkService.Connection.Closed -= OnClosed;

        // Views
        _friendsView.Dispose();
        GC.SuppressFinalize(this);
    }

    public override void Draw()
    {
        var spacing = ImGui.GetStyle().ItemSpacing;
        var windowPadding = ImGui.GetStyle().WindowPadding;
        var size = new Vector2(NavBarDimensions.X - windowPadding.X * 2, 25);
        var offset = windowPadding with { Y = (size.Y - ImGui.GetFontSize()) * 0.5f };
        
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, AetherRemoteStyle.Rounding);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, AetherRemoteStyle.Rounding);

        if (ImGui.BeginChild("###MainWindowNavBar", NavBarDimensions, true))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, AlignButtonTextLeft);

            if (_networkService.Connection.State is HubConnectionState.Connected || Plugin.DeveloperMode)
            {
                ImGui.TextUnformatted("General");
                NavBarButton(FontAwesomeIcon.User, "Status", _statusView, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.UserFriends, "Friends", _friendsView, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Globe, "Overrides", _overridesView, size, offset, spacing);

                ImGui.TextUnformatted("Control");
                NavBarButton(FontAwesomeIcon.Comments, "Speak", _speakView, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Smile, "Emote", _emoteView, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.WandMagicSparkles, "Transformation", _transformationView, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.PeopleArrows, "Body Swap", _bodySwapView, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.PeopleGroup, "Twinning", _twinningView, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Icons, "Moodles", _moodlesView, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Plus, "Customize", _customizePlusView, size, offset, spacing);
                NavBarButton(FontAwesomeIcon.Stopwatch, "Hypnosis", _hypnosisView, size, offset, spacing);

                ImGui.TextUnformatted("Configuration");
                NavBarButton(FontAwesomeIcon.History, "History", _historyView, size, offset, spacing);
            }
            else
            {
                ImGui.TextUnformatted("General");
                NavBarButton(FontAwesomeIcon.Plug, "Login", _loginView, size, offset, spacing);

                ImGui.TextUnformatted("Configuration");
            }

            NavBarButton(FontAwesomeIcon.Wrench, "Settings", _settingsView, size, offset, spacing);

            ImGui.PopStyleVar();
            ImGui.EndChild();
        }

        ImGui.PopStyleVar(2);
        ImGui.SameLine();

        if (_currentView.Draw() is false)
            return;
            

        ImGui.SameLine();
        var onFriendsView = _currentView == _friendsView;
        _friendsListComponent.Draw(onFriendsView, onFriendsView);
    }

    private void NavBarButton(FontAwesomeIcon icon, string text, IDrawable view, Vector2 size, Vector2 offset, Vector2 spacing)
    {
        var begin = ImGui.GetCursorPos();
        if (_currentView == view)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
            ImGui.Button($"##{text}", size);
            ImGui.PopStyleColor();
        }
        else
        {
            if (ImGui.Button($"##{text}", size))
            {
                _currentView = view;
                _friendsListService.PurgeOfflineFriendsFromSelect();
            }
        }
        
        ImGui.SetCursorPos(begin + offset);

        SharedUserInterfaces.Icon(icon);
        ImGui.SameLine();
        ImGui.TextUnformatted(text);
        ImGui.SetCursorPos(begin + new Vector2(0, size.Y + spacing.Y));
    }

    private Task OnConnected() => SetToStatusViewOrSettings();
    private Task OnReconnected(string? _) => SetToStatusViewOrSettings();
    private Task OnReconnecting(Exception? _) => SetToLoginViewOrSettings();
    private Task OnClosed(Exception? _) => SetToLoginViewOrSettings();

    private Task SetToStatusViewOrSettings()
    {
        if (_currentView == _loginView)
            _currentView = _statusView;

        return Task.CompletedTask;
    }

    private Task SetToLoginViewOrSettings()
    {
        if (_currentView != _settingsView)
            _currentView = _loginView;

        return Task.CompletedTask;
    }
}