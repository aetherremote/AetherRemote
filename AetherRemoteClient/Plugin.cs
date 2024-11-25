using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Accessors.Penumbra;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Domain.UI;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using System;
using System.Reflection;
using System.Threading.Tasks;
using AetherRemoteClient.Managers;

namespace AetherRemoteClient;

public sealed class Plugin : IDalamudPlugin
{
    // Lumina
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    
    // Game Objects
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    
    // Chat
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    
    // Dalamud
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;

    internal static Configuration Configuration { get; private set; } = null!;

    /// <summary>
    /// Plugin command name
    /// </summary>
    private const string CommandName = "/remote";

    /// <summary>
    /// Disables interacting with the server in any way, and returns mocked successes and the line when
    /// the server is invoked
    /// </summary>
#if DEBUG
    public const bool DeveloperMode = false;
#else
    public const bool DeveloperMode = false;
#endif
    
    /// <summary>
    /// Internal plugin stage
    /// </summary>
    public const string Stage = "Release";

    /// <summary>
    /// Internal plugin version
    /// </summary>
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);

    // Instantiated
    private ChatProvider ChatProvider { get; }
    private ClientDataManager ClientDataManager { get; }
    private HistoryLogProvider HistoryLogProvider { get; }
    private ModManager ModManager { get; }
    private SharedUserInterfaces SharedUserInterfaces { get; init; }
    private NetworkManager NetworkManager { get; }

    // Accessors
    private GlamourerAccessor GlamourerAccessor { get; }
    private PenumbraAccessor PenumbraAccessor { get; }

    // Providers
    private ActionQueueManager ActionQueueManager { get; }
    private EmoteProvider EmoteProvider { get; }
    private NetworkProvider NetworkProvider { get; }
    private WorldProvider WorldProvider { get; }

    // Windows
    private WindowSystem WindowSystem { get; }
    private MainWindow MainWindow { get; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Accessors
        GlamourerAccessor = new GlamourerAccessor();
        PenumbraAccessor = new PenumbraAccessor();

        // Manager
        
        SharedUserInterfaces = new SharedUserInterfaces();

        // Providers
        HistoryLogProvider = new HistoryLogProvider();
        ChatProvider = new ChatProvider();
        EmoteProvider = new EmoteProvider();
        WorldProvider = new WorldProvider();
        NetworkProvider = new NetworkProvider();
        
        // Manager
        ActionQueueManager = new ActionQueueManager(ChatProvider, HistoryLogProvider);
        ClientDataManager = new ClientDataManager(NetworkProvider);
        ModManager = new ModManager(PenumbraAccessor, GlamourerAccessor, NetworkProvider);
        NetworkManager = new NetworkManager(ActionQueueManager, ClientDataManager, EmoteProvider, GlamourerAccessor, HistoryLogProvider, ModManager, NetworkProvider, WorldProvider);
        
        // Windows
        WindowSystem = new WindowSystem("AetherRemote");
        MainWindow = new MainWindow(ActionQueueManager, ClientDataManager, EmoteProvider, GlamourerAccessor, HistoryLogProvider, ModManager, NetworkProvider, WorldProvider);
        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens primary Aether Remote window"
        });

        PluginInterface.UiBuilder.Draw += DrawUi;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        PluginInterface.UiBuilder.OpenConfigUi += OpenMainUi;

        if (DeveloperMode)
            MainWindow.IsOpen = true;
    }

    public async void Dispose()
    {
        PenumbraAccessor.Dispose();
        GlamourerAccessor.Dispose();

        NetworkProvider.Dispose();

        MainWindow.Dispose();
        WindowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandName);
        
        await ModManager.RemoveAllCollections();
        ModManager.Dispose();
        
        ClientDataManager.Dispose();

        PluginInterface.UiBuilder.Draw -= DrawUi;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
    }

    private void OnCommand(string command, string args)
    {
        MainWindow.IsOpen = true;
    }

    private void DrawUi()
    {
        // This way we can ensure all updates are taking place on the main thread
        ActionQueueManager.Update();

        WindowSystem.Draw();
    }

    private void OpenMainUi()
    {
        MainWindow.IsOpen = true;
    }

    /// <summary>
    /// Runs provided function on the XIV Framework. Await should never be utilized inside the <see cref="Func{T}"/> passed to this function.
    /// </summary>
    public static async Task<T> RunOnFramework<T>(Func<T> func)
    {
        if (Framework.IsInFrameworkUpdateThread)
            return func.Invoke();

        return await Framework.RunOnFrameworkThread(func).ConfigureAwait(false);
    }
}
