using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteClient.UI;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using XivCommon;
using XivCommon.Functions;

namespace AetherRemoteClient;

public sealed class Plugin : IDalamudPlugin
{
    private const string CommandName = "/remote";

    /// <summary>
    /// Disables interacting with the server in any way, and returns mocked successes and the line when
    /// the server is invoked.
    /// </summary>
    public static readonly bool DeveloperMode = true;

    /// <summary>
    /// Internal plugin stage
    /// </summary>
    public static readonly string Stage = "Alpha";

    /// <summary>
    /// Internal plugin version
    /// </summary>
    public static readonly string Version = "1.0.2.0";
    
    // Injected
    private DalamudPluginInterface pluginInterface { get; init; }
    private ICommandManager commandManager { get; init; }

    // Instantiated
    private Configuration configuration { get; init; }
    private SharedUserInterfaces sharedUserInterfaces { get; init; }
    private Chat chat { get; init; }

    // Accessors
    private GlamourerAccessor glamourerAccessor { get; init; }

    // Providers
    private ActionQueueProvider actionQueueProvider { get; init; }
    private EmoteProvider emoteProvider { get; init; }
    private NetworkProvider networkProvider { get; init; }

    // Listener
    private NetworkListener networkListener { get; init; }

    // Windows
    private WindowSystem windowSystem  { get; init; }
    private MainWindow mainWindow { get; init; }

    public Plugin(
        DalamudPluginInterface pluginInterface,
        ICommandManager commandManager,
        IAddonLifecycle addonLifecycle,
        ITargetManager targetManager,
        IClientState clientState,
        IDataManager dataManager,
        IPluginLog logger,
        IChatGui chatGUI,
        IObjectTable objectTable)
    {
        this.commandManager = commandManager;
        this.pluginInterface = pluginInterface;

        windowSystem = new WindowSystem("AetherRemote");

        configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        configuration.Initialize(pluginInterface);

        // Used in UI events
        sharedUserInterfaces = new SharedUserInterfaces(logger, pluginInterface);

        // Used to send messages to the server
        chat = new XivCommonBase(pluginInterface).Functions.Chat;

        // Accessors
        glamourerAccessor = new GlamourerAccessor(logger, pluginInterface);

        // Providers
        actionQueueProvider = new ActionQueueProvider(chat, glamourerAccessor, clientState, logger);
        emoteProvider = new EmoteProvider(dataManager);
        networkProvider = new NetworkProvider(logger);

        // Network Listener
        networkListener = new NetworkListener(actionQueueProvider, emoteProvider, networkProvider, logger);

        // Windows
        mainWindow = new MainWindow(configuration, emoteProvider, glamourerAccessor, 
            networkProvider, clientState, logger, targetManager);

        windowSystem.AddWindow(mainWindow);

        commandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens primary Aether Remote window"
        });

        pluginInterface.UiBuilder.Draw += DrawUI;
        pluginInterface.UiBuilder.OpenMainUi += OpenMainUI;

        if (DeveloperMode)
            mainWindow.IsOpen = true;
    }

    public void Dispose()
    {
        glamourerAccessor.Dispose();

        networkProvider.Dispose();

        windowSystem.RemoveAllWindows();
        commandManager.RemoveHandler(CommandName);

        pluginInterface.UiBuilder.Draw -= DrawUI;
        pluginInterface.UiBuilder.OpenMainUi -= OpenMainUI;
    }

    private void OnCommand(string command, string args)
    {
        mainWindow.IsOpen = true;
    }

    private void DrawUI()
    {
        // This way we can ensure all updates are taking place on the main thread
        actionQueueProvider.Update();

        windowSystem.Draw();
    }

    private void OpenMainUI()
    {
        mainWindow.IsOpen = true;
    }
}
