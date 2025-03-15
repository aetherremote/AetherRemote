using System;
using System.Reflection;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Handlers;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.External;
using AetherRemoteClient.UI;
using AetherRemoteClient.Utils;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;

namespace AetherRemoteClient;

// ReSharper disable once ClassNeverInstantiated.Global
public sealed class Plugin : IDalamudPlugin
{
    // Lumina
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;

    // Game Objects
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;

    // Chat
    [PluginService] private static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;

    // Dalamud
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] private static IFramework Framework { get; set; } = null!;
    [PluginService] internal static INotificationManager NotificationManager { get; private set; } = null!;
    internal static Configuration Configuration { get; private set; } = null!;

    /// <summary>
    ///     Plugin command name
    /// </summary>
    private const string CommandName = "/remote";

    /// <summary>
    ///     Disables interacting with the server in any way, and returns mocked successes and the line when
    ///     the server is invoked
    /// </summary>
#if DEBUG
    public const bool DeveloperMode = false;
#else
    public const bool DeveloperMode = false;
#endif

    /// <summary>
    ///     Internal plugin version
    /// </summary>
    public static readonly Version Version =
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

    // Windows
    private WindowSystem WindowSystem { get; }
    private MainWindow MainWindow { get; }

    // Disposable Services
    private readonly IdentityService _identityService;
    private readonly NetworkService _networkService;

    // Disposable External Services
    private readonly GlamourerService _glamourerService;
    private readonly PenumbraService _penumbraService;

    // Managers
    private readonly ActionQueueManager _actionQueueManager;

    // Disposable Managers
    private readonly ConnectivityManager _connectivityManager;
    private readonly ModManager _modManager;

    // Disposable Handlers
    private readonly NetworkHandler _networkHandler;
    
    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Services
        var chatService = new ChatService();
        var commandLockoutService = new CommandLockoutService();
        var emoteService = new EmoteService();
        var friendsListService = new FriendsListService();
        var logService = new LogService();
        var overrideService = new OverrideService();
        var worldService = new WorldService();

        // Disposable Services
        _identityService = new IdentityService();
        _networkService = new NetworkService();

        // External Services
        var moodlesService = new MoodlesService();
        _glamourerService = new GlamourerService();
        _penumbraService = new PenumbraService();

        // Managers
        _actionQueueManager = new ActionQueueManager(chatService);
        _connectivityManager = new ConnectivityManager(friendsListService, _identityService, _networkService);
        _modManager = new ModManager(_glamourerService, moodlesService, _penumbraService);

        // Handlers
        _networkHandler = new NetworkHandler(chatService, emoteService, friendsListService, _glamourerService,
            _identityService, moodlesService, overrideService, _penumbraService, logService, _networkService, _actionQueueManager,
            _modManager);

        // Windows
        MainWindow = new MainWindow(commandLockoutService, emoteService, friendsListService, _glamourerService,
            _identityService, logService, moodlesService, _networkService, overrideService, worldService, _modManager);

        WindowSystem = new WindowSystem("AetherRemote");
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens primary Aether Remote window"
        });

        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi += OpenMainUi;

        Task.Run(SharedUserInterfaces.InitializeFonts);

        if (DeveloperMode)
            MainWindow.IsOpen = true;
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.IsOpen = true;
    }

    private void DrawUi()
    {
        AetherRemoteStyle.Stylize();
        WindowSystem.Draw();
        AetherRemoteStyle.UnStylize();

        // Since we should always be on the main thread when sending a message, it is useful to update it here
        _actionQueueManager.Update();
    }

    private void OpenMainUi()
    {
        MainWindow.IsOpen = true;
    }

    /// <summary>
    ///     Runs provided function on the XIV Framework. Await should never be used inside the <see cref="Func{T}"/>
    ///     passed to this function.
    /// </summary>
    public static async Task<T> RunOnFramework<T>(Func<T> func)
    {
        if (Framework.IsInFrameworkUpdateThread)
            return func.Invoke();

        return await Framework.RunOnFrameworkThread(func).ConfigureAwait(false);
    }

    /// <summary>
    ///     Runs provided function on the XIV Framework. Await should never be used inside the <see cref="Action"/>
    ///     passed to this function.
    /// </summary>
    public static async Task RunOnFramework(Action action)
    {
        if (Framework.IsInFrameworkUpdateThread)
            action.Invoke();

        await Framework.RunOnFrameworkThread(action).ConfigureAwait(false);
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= DrawUi;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenMainUi;

        // Services
        _identityService.Dispose();
        _networkService.Dispose();

        // External Services
        _glamourerService.Dispose();
        _penumbraService.Dispose();

        // Managers
        _connectivityManager.Dispose();
        _modManager.Dispose();

        // Handlers
        _networkHandler.Dispose();

        // Windows
        MainWindow.Dispose();

        WindowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandName);
    }

    /*
     *  AR Supporters Name-Game
     *  =======================
     *  I want to show appreciation for those who were here in the beginning, supporting both the plugin
     *  and I unconditionally. There have been a lot of tough challenges and fun moments,
     *  but you all helped me preserve and that deserves recognition.
     *  So I've decided to immorality all those names in the plugin code; Not as comments, but as actual variables!
     *  Below is a list of everyone who will slowly be phased into variable names, see if you can spot where they appear
     *  in future commits! Looking at you, Tezra.
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