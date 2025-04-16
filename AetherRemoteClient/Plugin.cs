using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Handlers;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI;
using AetherRemoteClient.Utils;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
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
    [PluginService] internal static IChatGui ChatGui { get; private set; } = null!;

    // Dalamud
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; set; } = null!;
    [PluginService] internal static INotificationManager NotificationManager { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    internal static Configuration Configuration { get; private set; } = null!;

    /// <summary>
    ///     Disables interacting with the server in any way, and returns mocked successes and the line when
    ///     the server is invoked
    /// </summary>
#if DEBUG
    public const bool DeveloperMode = true;
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
    private readonly SpiralService _spiralService;

    // IPCs
    private readonly CustomizePlusIpc _customizePlusIpc;
    private readonly GlamourerIpc _glamourerIpc;

    // Managers
    private readonly ActionQueueManager _actionQueueManager;
    private readonly DependencyManager _dependencyManager;

    // Disposable Managers
    private readonly ConnectivityManager _connectivityManager;
    private readonly ModManager _modManager;

    // Disposable Handlers
    private readonly NetworkHandler _networkHandler;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Services
        var commandLockoutService = new CommandLockoutService();
        var emoteService = new EmoteService();
        var friendsListService = new FriendsListService();
        _identityService = new IdentityService();
        var logService = new LogService();
        _networkService = new NetworkService();
        var overrideService = new OverrideService();
        _spiralService = new SpiralService();
        var tipService = new TipService();
        var worldService = new WorldService();

        // IPCs
        _customizePlusIpc = new CustomizePlusIpc();
        _glamourerIpc = new GlamourerIpc();
        var moodlesIpc = new MoodlesIpc();
        var penumbraIpc = new PenumbraIpc();

        // Managers
        _actionQueueManager = new ActionQueueManager();
        _connectivityManager = new ConnectivityManager(friendsListService, _identityService, _networkService);
        _dependencyManager = new DependencyManager(_customizePlusIpc, _glamourerIpc, moodlesIpc, penumbraIpc);
        _modManager = new ModManager(_customizePlusIpc, _glamourerIpc, moodlesIpc, penumbraIpc);

        // Handlers
        _networkHandler = new NetworkHandler(_customizePlusIpc, emoteService, friendsListService, _identityService,
            overrideService, logService, _networkService, _spiralService, _glamourerIpc, moodlesIpc, penumbraIpc, _actionQueueManager,
            _modManager);

        // Windows
        MainWindow = new MainWindow(commandLockoutService, emoteService, friendsListService, _identityService,
            logService, _networkService, overrideService, _spiralService, tipService, worldService, _customizePlusIpc, _glamourerIpc,
            moodlesIpc, penumbraIpc, _actionQueueManager, _modManager);

        WindowSystem = new WindowSystem("AetherRemote");
        WindowSystem.AddWindow(MainWindow);
        
        BuildCommands();

        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi += OpenMainUi;

        Task.Run(SharedUserInterfaces.InitializeFonts);

#if DEBUG
        MainWindow.IsOpen = true;
#endif
    }

    private const string OldCommandName = "/remote";
    private const string CommandNameFull = "/aetherremote";
    private const string CommandNameShort = "/ar";

    private const string StopArg = "stop";
    private const string SafeMode = "safemode";
    private const string SafeWord = "safeword";

    private void BuildCommands()
    {
        CommandManager.AddHandler(CommandNameShort, new CommandInfo(OnCommand)
        {
            HelpMessage = $"""
                          Opens the primary plugin window
                          /ar {StopArg} - Stops all current spirals
                          /ar {SafeMode} - Put the plugin into safe mode
                          /ar {SafeWord} - Put the plugin into safe mode
                          """
        });
        
        CommandManager.AddHandler(CommandNameFull, new CommandInfo(OnCommand)
        {
            ShowInHelp = false
        });
        
        CommandManager.AddHandler(OldCommandName, new CommandInfo(OnCommand)
        {
            ShowInHelp = false
        });
    }

    private void OnCommand(string command, string args)
    {
        if (args == string.Empty)
        {
            MainWindow.IsOpen = true;
            return;
        }

        var payloads = new List<Payload>();
        switch (args)
        {
            case StopArg:
                _spiralService.StopCurrentSpiral();
                payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorPurple));
                payloads.Add(new TextPayload("[AetherRemote] "));
                payloads.Add(UIForegroundPayload.UIForegroundOff);
                payloads.Add( new TextPayload("Stopped current spirals"));
                break;
            
            case SafeMode:
            case SafeWord:
                _spiralService.StopCurrentSpiral();
                _actionQueueManager.Clear();
                Configuration.SafeMode = true;
                Configuration.Save();
                payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorPurple));
                payloads.Add(new TextPayload("[AetherRemote] "));
                payloads.Add(UIForegroundPayload.UIForegroundOff);
                payloads.Add(new TextPayload("Plugin is now in "));
                payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorGreen));
                payloads.Add(new TextPayload("safe mode"));
                payloads.Add(UIForegroundPayload.UIForegroundOff);
                break;
            
            default:
                payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorPurple));
                payloads.Add(new TextPayload("[AetherRemote] "));
                payloads.Add(UIForegroundPayload.UIForegroundOff);
                payloads.Add(new TextPayload($"Unknown argument \"{args}\""));
                break;
        }
        
        if (payloads.Count > 0)
            ChatGui.Print(new SeString(payloads));
    }

    private void DrawUi()
    {
        AetherRemoteStyle.Stylize();
        WindowSystem.Draw();
        AetherRemoteStyle.UnStylize();

        // Since we should always be on the main thread when sending a message, it is useful to update it here
        _actionQueueManager.Update();
        _dependencyManager.Update();
        _spiralService.DrawSpiral();
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

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= DrawUi;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenMainUi;

        // Services
        _identityService.Dispose();
        _networkService.Dispose();

        // External Services
        _glamourerIpc.Dispose();
        _customizePlusIpc.Dispose();

        // Managers
        _connectivityManager.Dispose();
        _modManager.Dispose();

        // Handlers
        _networkHandler.Dispose();

        // Windows
        MainWindow.Dispose();

        WindowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandNameShort);
        CommandManager.RemoveHandler(CommandNameFull);
        CommandManager.RemoveHandler(OldCommandName);
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