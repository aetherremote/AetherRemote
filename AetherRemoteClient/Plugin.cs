using AetherRemoteClient.Accessors.Glamourer;
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

namespace AetherRemoteClient;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;

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
    public const bool DeveloperMode = true;
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
    private Chat chat { get; init; }
    private ClientDataManager clientDataManager { get; init; }
    private HistoryLogManager historyLogManager { get; set; }
    private SharedUserInterfaces sharedUserInterfaces { get; init; }

    // Accessors
    private GlamourerAccessor glamourerAccessor { get; init; }

    // Providers
    private ActionQueueProvider actionQueueProvider { get; init; }
    private EmoteProvider emoteProvider { get; init; }
    private NetworkProvider networkProvider { get; init; }
    private WorldProvider worldProvider { get; init; }

    // Windows
    private WindowSystem windowSystem { get; init; }
    private MainWindow mainWindow { get; init; }

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        // Instantiate
        chat = new Chat();
        clientDataManager = new ClientDataManager();
        historyLogManager = new HistoryLogManager();
        sharedUserInterfaces = new SharedUserInterfaces();

        // Accessors
        glamourerAccessor = new GlamourerAccessor();

        // Providers
        actionQueueProvider = new ActionQueueProvider(chat, historyLogManager);
        emoteProvider = new EmoteProvider();
        networkProvider = new NetworkProvider(actionQueueProvider, clientDataManager, emoteProvider, glamourerAccessor, historyLogManager);
        worldProvider = new WorldProvider();

        // Windows
        windowSystem = new WindowSystem("AetherRemote");
        mainWindow = new MainWindow(actionQueueProvider, clientDataManager, emoteProvider, glamourerAccessor, historyLogManager, networkProvider, worldProvider);
        windowSystem.AddWindow(mainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Opens primary Aether Remote window"
        });

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUI;
        PluginInterface.UiBuilder.OpenConfigUi += OpenMainUI;

        if (DeveloperMode)
            mainWindow.IsOpen = true;
    }

    public void Dispose()
    {
        glamourerAccessor.Dispose();

        networkProvider.Dispose();

        mainWindow.Dispose();
        windowSystem.RemoveAllWindows();
        CommandManager.RemoveHandler(CommandName);

        PluginInterface.UiBuilder.Draw -= DrawUI;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUI;
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
