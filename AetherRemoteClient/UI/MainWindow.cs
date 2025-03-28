using System;
using System.Numerics;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.External;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.UI.Views.BodySwap;
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

    // Injected
    private readonly FriendsListService _friendsListService;
    private readonly NetworkService _networkService;

    // Components
    private readonly FriendsListComponentUi _friendsListComponent;

    // Views
    private readonly BodySwapViewUi _bodySwapView;
    private readonly EmoteViewUi _emoteView;
    private readonly FriendsViewUi _friendsView;
    private readonly HistoryViewUi _historyView;
    private readonly LoginViewUi _loginView;
    private readonly MoodlesViewUi _moodlesView;
    private readonly OverridesViewUi _overridesView;
    private readonly SettingsViewUi _settingsView;
    private readonly SpeakViewUi _speakView;
    private readonly StatusViewUi _statusView;
    private readonly TransformationViewUi _transformationView;
    private readonly TwinningViewUi _twinningView;
    private readonly HypnosisViewUi _hypnosisView;
    private IDrawable _currentView;

    // Instantiated
    private Vector2 _dimensions = Vector2.Zero;
    private float _itemSpacing;
    private Vector2 _offset = Vector2.Zero;

    public MainWindow(
        CommandLockoutService commandLockoutService,
        EmoteService emoteService,
        FriendsListService friendsListService,
        GlamourerService glamourerService,
        IdentityService identityService,
        LogService logService,
        NetworkService networkService,
        OverrideService overrideService,
        TipService tipService,
        WorldService worldService,
        ModManager modManager) : base("Aether Remote 2")
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 500),
            MaximumSize = ImGui.GetIO().DisplaySize
        };

        // Components
        _friendsListComponent = new FriendsListComponentUi(friendsListService, networkService);

        // Views
        _statusView = new StatusViewUi(glamourerService, networkService, identityService, tipService);
        _friendsView = new FriendsViewUi(friendsListService, networkService);
        _overridesView = new OverridesViewUi(overrideService);
        _speakView = new SpeakViewUi(commandLockoutService, friendsListService, networkService, worldService);
        _emoteView = new EmoteViewUi(commandLockoutService, emoteService, friendsListService, networkService);
        _transformationView =
            new TransformationViewUi(commandLockoutService, glamourerService, friendsListService, networkService);
        _bodySwapView = new BodySwapViewUi(commandLockoutService, identityService, friendsListService, networkService,
            modManager);
        _twinningView = new TwinningViewUi(commandLockoutService, friendsListService, identityService, networkService);
        _historyView = new HistoryViewUi(logService);
        _settingsView = new SettingsViewUi();
        _loginView = new LoginViewUi(networkService);
        _moodlesView = new MoodlesViewUi(commandLockoutService, friendsListService, networkService);
        _hypnosisView = new HypnosisViewUi(friendsListService);

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
        var style = ImGui.GetStyle();
        var windowPadding = style.WindowPadding;
        _dimensions = new Vector2(NavBarDimensions.X - windowPadding.X * 2, 25);
        _offset = windowPadding with { Y = (_dimensions.Y - ImGui.GetFontSize()) * 0.5f };
        _itemSpacing = style.ItemSpacing.Y;

        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, AetherRemoteStyle.Rounding);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, AetherRemoteStyle.Rounding);

        if (ImGui.BeginChild("###MainWindowNavBar", NavBarDimensions, true))
        {
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, AlignButtonTextLeft);

            if (_networkService.Connection.State is HubConnectionState.Connected || Plugin.DeveloperMode)
            {
                ImGui.TextUnformatted("General");
                CreateNavBarButton(FontAwesomeIcon.User, "Status", _statusView);
                CreateNavBarButton(FontAwesomeIcon.UserFriends, "Friends", _friendsView);
                CreateNavBarButton(FontAwesomeIcon.Globe, "Overrides", _overridesView);

                ImGui.TextUnformatted("Control");
                CreateNavBarButton(FontAwesomeIcon.Comments, "Speak", _speakView);
                CreateNavBarButton(FontAwesomeIcon.Smile, "Emote", _emoteView);
                CreateNavBarButton(FontAwesomeIcon.WandMagicSparkles, "Transformation", _transformationView);
                CreateNavBarButton(FontAwesomeIcon.PeopleArrows, "Body Swap", _bodySwapView);
                CreateNavBarButton(FontAwesomeIcon.PeopleGroup, "Twinning", _twinningView);
                CreateNavBarButton(FontAwesomeIcon.Icons, "Moodles", _moodlesView);
                CreateNavBarButton(FontAwesomeIcon.Stopwatch, "Hypnosis", _hypnosisView);

                ImGui.TextUnformatted("Configuration");
                CreateNavBarButton(FontAwesomeIcon.History, "History", _historyView);
            }
            else
            {
                ImGui.TextUnformatted("General");
                CreateNavBarButton(FontAwesomeIcon.Plug, "Login", _loginView);

                ImGui.TextUnformatted("Configuration");
            }

            CreateNavBarButton(FontAwesomeIcon.Wrench, "Settings", _settingsView);

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

    private void CreateNavBarButton(FontAwesomeIcon icon, string text, IDrawable view)
    {
        var begin = ImGui.GetCursorPos();

        bool clicked;
        if (_currentView == view)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
            clicked = ImGui.Button($"##{text}", _dimensions);
            ImGui.PopStyleColor();
        }
        else
        {
            clicked = ImGui.Button($"##{text}", _dimensions);
        }

        ImGui.SetCursorPos(begin + _offset);

        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();

        ImGui.SameLine();
        ImGui.TextUnformatted(text);

        ImGui.SetCursorPos(begin + new Vector2(0, _dimensions.Y + _itemSpacing));

        if (clicked is false)
            return;

        // Set view
        _currentView = view;
        if (_currentView == _friendsView)
            return;

        // If view isn't friends list, purge any offline friends from selection
        _friendsListService.PurgeOfflineFriendsFromSelect();
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