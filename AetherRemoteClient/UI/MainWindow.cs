using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.NavigationBar;
using AetherRemoteClient.UI.Views.BodySwap;
using AetherRemoteClient.UI.Views.CustomizePlus;
using AetherRemoteClient.UI.Views.Emote;
using AetherRemoteClient.UI.Views.Friends;
using AetherRemoteClient.UI.Views.History;
using AetherRemoteClient.UI.Views.Hypnosis;
using AetherRemoteClient.UI.Views.Login;
using AetherRemoteClient.UI.Views.Moodles;
using AetherRemoteClient.UI.Views.Pause;
using AetherRemoteClient.UI.Views.Possession;
using AetherRemoteClient.UI.Views.Settings;
using AetherRemoteClient.UI.Views.Speak;
using AetherRemoteClient.UI.Views.Status;
using AetherRemoteClient.UI.Views.Transformation;
using AetherRemoteClient.UI.Views.Twinning;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;

namespace AetherRemoteClient.UI;

public class MainWindow : Window
{
    // Const
    private static readonly string MainWindowTitle = $"Aether Remote 2 - Version {Plugin.Version}";

    // Services
    private readonly ViewService _viewService;

    // Components
    private readonly NavigationBarComponentUi _navigationBar;

    // Views
    private readonly BodySwapViewUi _bodySwapView;
    private readonly CustomizePlusViewUi _customizePlusView;
    private readonly EmoteViewUi _emoteView;
    private readonly FriendsViewUi _friendsView;
    private readonly HistoryViewUi _historyView;
    private readonly HypnosisViewUi _hypnosisView;
    private readonly LoginViewUi _loginView;
    private readonly MoodlesViewUi _moodlesView;
    private readonly PauseViewUi _pauseView;
    private readonly PossessionViewUi _possessionView;
    private readonly SettingsViewUi _settingsView;
    private readonly SpeakViewUi _speakView;
    private readonly StatusViewUi _statusView;
    private readonly TransformationViewUi _transformationView;
    private readonly TwinningViewUi _twinningView;

    public MainWindow(
        ViewService viewService,
        NavigationBarComponentUi navigationBarComponentUi,
        BodySwapViewUi bodySwapView,
        CustomizePlusViewUi customizePlusView,
        EmoteViewUi emoteView,
        FriendsViewUi friendsView,
        HistoryViewUi historyView,
        HypnosisViewUi hypnosisView,
        LoginViewUi loginView,
        MoodlesViewUi moodlesView,
        PauseViewUi pauseView,
        PossessionViewUi possessionView,
        SettingsViewUi settingsView,
        SpeakViewUi speakView,
        StatusViewUi statusView,
        TransformationViewUi transformationView,
        TwinningViewUi twinningView) : base(MainWindowTitle)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(800, 500),
            MaximumSize = ImGui.GetIO().DisplaySize
        };

        _viewService = viewService;

        _navigationBar = navigationBarComponentUi;

        _bodySwapView = bodySwapView;
        _customizePlusView = customizePlusView;
        _emoteView = emoteView;
        _friendsView = friendsView;
        _historyView = historyView;
        _hypnosisView = hypnosisView;
        _loginView = loginView;
        _moodlesView = moodlesView;
        _pauseView = pauseView;
        _possessionView = possessionView;
        _settingsView = settingsView;
        _speakView = speakView;
        _statusView = statusView;
        _transformationView = transformationView;
        _twinningView = twinningView;
    }

    public override void Draw()
    {
        _navigationBar.Draw();

        ImGui.SameLine();

        IDrawable view = _viewService.CurrentView switch
        {
            View.BodySwap => _bodySwapView,
            View.CustomizePlus => _customizePlusView,
            View.Emote => _emoteView,
            View.Friends => _friendsView,
            View.History => _historyView,
            View.Hypnosis => _hypnosisView,
            View.Login => _loginView,
            View.Moodles => _moodlesView,
            View.Pause => _pauseView,
            View.Possession => _possessionView,
            View.Settings => _settingsView,
            View.Speak => _speakView,
            View.Status => _statusView,
            View.Transformation => _transformationView,
            View.Twinning => _twinningView,
            _ => _loginView
        };

        view.Draw();
    }
}