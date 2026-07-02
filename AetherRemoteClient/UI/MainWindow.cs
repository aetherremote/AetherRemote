using System;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.NavigationBar;
using AetherRemoteClient.UI.Views.CustomizePlus;
using AetherRemoteClient.UI.Views.Debug;
using AetherRemoteClient.UI.Views.Emote;
using AetherRemoteClient.UI.Views.Friends;
using AetherRemoteClient.UI.Views.History;
using AetherRemoteClient.UI.Views.Home;
using AetherRemoteClient.UI.Views.Honorific;
using AetherRemoteClient.UI.Views.Hypnosis;
using AetherRemoteClient.UI.Views.Login;
using AetherRemoteClient.UI.Views.Moodles;
using AetherRemoteClient.UI.Views.Pause;
using AetherRemoteClient.UI.Views.Possession;
using AetherRemoteClient.UI.Views.Settings;
using AetherRemoteClient.UI.Views.Speak;
using AetherRemoteClient.UI.Views.Status;
using AetherRemoteClient.UI.Views.Transformations;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace AetherRemoteClient.UI;

public class MainWindow : Window, IDisposable
{
    // Const
    private static readonly string MainWindowTitle = $"Aether Remote 2 - Version {Plugin.Version}";

    // Services
    private readonly ViewService _viewService;
    
    // Handlers
    private readonly DtrManager _dtrManager;

    // Components
    private readonly NavigationBarComponentUi _navigationBar;

    // Views
    private readonly CustomizePlusView _customizePlusView;
    private readonly DebugView _debugView;
    private readonly EmoteView _emoteView;
    private readonly FriendsView _friendsView;
    private readonly HistoryView _historyView;
    private readonly HomeView _homeView;
    private readonly HonorificView _honorificView;
    private readonly HypnosisView _hypnosisView;
    private readonly LoginView _loginView;
    private readonly MoodlesView _moodlesView;
    private readonly PauseView _pauseView;
    private readonly PossessionView _possessionView;
    private readonly SettingsView _settingsView;
    private readonly SpeakView _speakView;
    private readonly StatusView _statusView;
    private readonly TransformationsView _transformationsView;

    public MainWindow(
        ViewService viewService,
        DtrManager dtrManager,
        NavigationBarComponentUi navigationBarComponentUi,
        CustomizePlusView customizePlusView,
        DebugView debugView,
        EmoteView emoteView,
        FriendsView friendsView,
        HistoryView historyView,
        HomeView homeView,
        HonorificView honorificView,
        HypnosisView hypnosisView,
        LoginView loginView,
        MoodlesView moodlesView,
        PossessionView possessionView,
        PauseView pauseView,
        SettingsView settingsView,
        SpeakView speakView,
        StatusView statusView,
        TransformationsView transformationsView) : base(MainWindowTitle)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 500),
            MaximumSize = ImGui.GetIO().DisplaySize
        };
        
        _viewService = viewService;
        _dtrManager = dtrManager;

        _navigationBar = navigationBarComponentUi;
        
        _customizePlusView = customizePlusView;
        _debugView = debugView;
        _emoteView = emoteView;
        _friendsView = friendsView;
        _historyView = historyView;
        _homeView = homeView;
        _honorificView = honorificView;
        _hypnosisView = hypnosisView;
        _loginView = loginView;
        _moodlesView = moodlesView;
        _pauseView = pauseView;
        _possessionView =  possessionView;
        _settingsView = settingsView;
        _speakView = speakView;
        _statusView = statusView;
        _transformationsView = transformationsView;

        _dtrManager.DtrClicked += OnDtrClicked;
    }

    public override void Draw()
    {
        _navigationBar.Draw();

        ImGui.SameLine();

        IDrawable view = _viewService.CurrentView switch
        {
            View.CustomizePlus => _customizePlusView,
            View.Home => _homeView,
            View.Debug => _debugView,
            View.Emote => _emoteView,
            View.Friends => _friendsView,
            View.History => _historyView,
            View.Honorific => _honorificView,
            View.Hypnosis => _hypnosisView,
            View.Login => _loginView,
            View.Moodles => _moodlesView,
            View.Pause => _pauseView,
            View.Possession => _possessionView,
            View.Settings => _settingsView,
            View.Speak => _speakView,
            View.Status => _statusView,
            View.Transformations => _transformationsView,
            _ => _loginView
        };

        view.Draw();
    }
    
    private void OnDtrClicked()
    {
        IsOpen = true;
    }

    public void Dispose()
    {
        _dtrManager.DtrClicked -= OnDtrClicked;
        GC.SuppressFinalize(this);
    }
}