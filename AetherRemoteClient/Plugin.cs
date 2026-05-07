using System;
using System.Reflection;
using AetherRemoteClient.Domain.Configurations;
using AetherRemoteClient.Handlers;
using AetherRemoteClient.Handlers.Chat;
using AetherRemoteClient.Handlers.Network;
using AetherRemoteClient.Hooks;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.UI.Components.NavigationBar;
using AetherRemoteClient.UI.Views.CustomizePlus;
using AetherRemoteClient.UI.Views.Debug;
using AetherRemoteClient.UI.Views.Emote;
using AetherRemoteClient.UI.Views.Friends;
using AetherRemoteClient.UI.Views.Friends.Ui;
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
using AetherRemoteClient.Utils;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AetherRemoteClient;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static INotificationManager NotificationManager { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig { get; private set; } = null!;
    [PluginService] internal static IDtrBar DtrBar { get; private set; } = null!;
    internal static Configuration Configuration { get; private set; } = null!;
    internal static CharacterConfiguration? CharacterConfiguration { get; set; }

    /// <summary>
    ///     Internal plugin version
    /// </summary>
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    
    // Instantiated
    private readonly ServiceProvider _services;
    
    public Plugin()
    {
        // Load the default configuration
        Configuration = ConfigurationService.LoadConfiguration().GetAwaiter().GetResult() ?? new Configuration();
        
        // Create a collection of services
        var services = new ServiceCollection();
        
        // Services
        services.AddSingleton<AccountService>();
        services.AddSingleton<ActionQueueService>();
        services.AddSingleton<CommandLockoutService>();
        services.AddSingleton<EmoteService>();
        services.AddSingleton<FriendsListService>();
        services.AddSingleton<GameSettingsService>();
        services.AddSingleton<LogService>();
        services.AddSingleton<NetworkService>();
        services.AddSingleton<PauseService>();
        services.AddSingleton<StatusManager>();
        services.AddSingleton<TipService>();
        services.AddSingleton<ViewService>();
        services.AddSingleton<WorldService>();
        
        // Services - Dependencies
        services.AddSingleton<CustomizePlusService>();
        services.AddSingleton<GlamourerService>();
        services.AddSingleton<HonorificService>();
        services.AddSingleton<MoodlesService>();
        services.AddSingleton<PenumbraService>();
        
        // Hooks
        services.AddSingleton<CameraHook>();
        services.AddSingleton<CameraInputHook>();
        services.AddSingleton<CameraTargetHook>();
        services.AddSingleton<MovementInputHook>();
        services.AddSingleton<MovementHook>();
        services.AddSingleton<MovementLockHook>();
        
        // Managers
        services.AddSingleton<CharacterTransformationManager>();
        services.AddSingleton<ConnectionManager>();
        services.AddSingleton<DependencyManager>();
        services.AddSingleton<HypnosisManager>();
        services.AddSingleton<LoginManager>();
        services.AddSingleton<NetworkCommandManager>();
        services.AddSingleton<PossessionManager>();
        services.AddSingleton<SelectionManager>();
        
        // Handlers
        services.AddSingleton<ChatCommandHandler>();
        services.AddSingleton<DtrHandler>();
        services.AddSingleton<GlamourerEventHandler>();
        
        // Handlers Network
        services.AddSingleton<NetworkHandler>();
        
        // Ui - Component Controllers
        services.AddSingleton<FriendsListComponentUiController>();
        
        // Ui - Components
        services.AddSingleton<FriendsListComponentUi>();
        services.AddSingleton<NavigationBarComponentUi>();
        
        // Ui - View Controllers
        services.AddSingleton<CustomizePlusViewUiController>();
        services.AddSingleton<HomeViewUiController>();
        services.AddSingleton<DebugViewUiController>();
        services.AddSingleton<EmoteViewUiController>();
        services.AddSingleton<FriendsViewUiController>();
        services.AddSingleton<HistoryViewUiController>();
        services.AddSingleton<HonorificViewUiController>();
        services.AddSingleton<HypnosisViewUiController>();
        services.AddSingleton<LoginViewUiController>();
        services.AddSingleton<MoodlesViewUiController>();
        services.AddSingleton<PauseViewUiController>();
        services.AddSingleton<PossessionViewUiController>();
        services.AddSingleton<SettingsViewUiController>();
        services.AddSingleton<SpeakViewUiController>();
        services.AddSingleton<StatusViewUiController>();
        services.AddSingleton<TransformationsViewUiController>();
        
        // Ui - Views
        services.AddSingleton<CustomizePlusViewUi>();
        services.AddSingleton<HomeViewUi>();
        services.AddSingleton<DebugViewUi>();
        services.AddSingleton<EmoteViewUi>();
        services.AddSingleton<FriendsViewUi>();
        services.AddSingleton<HistoryViewUi>();
        services.AddSingleton<HonorificViewUi>();
        services.AddSingleton<HypnosisViewUi>();
        services.AddSingleton<LoginViewUi>();
        services.AddSingleton<MoodlesViewUi>();
        services.AddSingleton<PauseViewUi>();
        services.AddSingleton<PossessionViewUi>();
        services.AddSingleton<SettingsViewUi>();
        services.AddSingleton<SpeakViewUi>();
        services.AddSingleton<StatusViewUi>();
        services.AddSingleton<TransformationsViewUi>();
        
        // Ui - Windows
        services.AddSingleton<MainWindow>();
        services.AddSingleton<WindowManager>();
        
        // Build the dependency injection framework
        _services = services.BuildServiceProvider();
        
        // Ui - Windows
        _services.GetRequiredService<WindowManager>();
        
        // Ui - Controllers
        _services.GetRequiredService<LoginViewUiController>();              // Required to display secret once character configuration loads
        _services.GetRequiredService<MoodlesViewUiController>();            // Required to display UI elements when IPCs are loaded
        _services.GetRequiredService<TransformationsViewUiController>();    // Required to display UI elements when IPCs are loaded
        _services.GetRequiredService<CustomizePlusViewUiController>();      // Required to display UI elements when IPCs are loaded
        _services.GetRequiredService<HonorificViewUiController>();          // Required to display UI elements when IPCs are loaded
        
        // Handlers
        _services.GetRequiredService<ChatCommandHandler>();
        _services.GetRequiredService<ConnectionManager>();
        _services.GetRequiredService<DtrHandler>();
        _services.GetRequiredService<GlamourerEventHandler>();
        
        // Handlers Network
        _services.GetRequiredService<NetworkHandler>();
        
        // Managers
        _services.GetRequiredService<DependencyManager>();
        _services.GetRequiredService<HypnosisManager>();
        _services.GetRequiredService<LoginManager>();
        _services.GetRequiredService<PossessionManager>();
        
        // Services
        _services.GetRequiredService<ActionQueueService>();
        
        _ = SharedUserInterfaces.InitializeFonts().ConfigureAwait(false);
    }
    
    public void Dispose()
    {
        _services.Dispose();
    }

    /*
     *  AR Supporters Name-Game
     *  =======================
     *  I want to show appreciation for those who were here in the beginning, supporting both the plugin
     *  and I unconditionally. There have been a lot of tough challenges and fun moments,
     *  but you all helped me preserve and that deserves recognition.
     *  So I've decided to immorality all those names in the plugin code; Not as comments, but as actual variables!
     *  Below is a list of everyone who will slowly be phased into variable names, see if you can spot where they appear
     *  in future commits! I'm looking at you, Tezra.
     *  Much love to every name on this list. If I missed anyone, PLEASE LET ME KNOW. There were a lot of people to comb
     *  through, and I may have missed a name or two.
     *  =======================
     *  Aria
     *  Asami
     *  Cami
     *  Clarjii
     *  Cleichant
     *  Damy
     *  Delilah
     *  Dub
     *  Etche
     *  Eleanora
     *  Ferra
     *  Kaga
     *  Kari
     *  Kerc
     *  Leona
     *  Mae
     *  Misty
     *  Miyuki
     *  Mylla
     *  Neith
     *  Norg
     *  Pet
     *  Pris
     *  Red
     *  Rosalyne
     *  Silent
     *  Soph
     *  Suzy
     *  Tezra
     *  Tixa/Dolly
     *  Traia
     *  Vanessa
     *  Yilana
     */
}