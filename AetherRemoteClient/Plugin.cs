using System;
using System.Reflection;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Handlers;
using AetherRemoteClient.Handlers.Network;
using AetherRemoteClient.Ipc;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI;
using AetherRemoteClient.UI.Components.Friends;
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
using AetherRemoteClient.UI.Views.Settings;
using AetherRemoteClient.UI.Views.Speak;
using AetherRemoteClient.UI.Views.Status;
using AetherRemoteClient.UI.Views.Transformation;
using AetherRemoteClient.UI.Views.Twinning;
using AetherRemoteClient.Utils;
using Dalamud.Game.ClientState.Objects;
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
    [PluginService] internal static IFramework Framework { get; set; } = null!;
    [PluginService] internal static INotificationManager NotificationManager { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    internal static Configuration Configuration { get; private set; } = null!;

    /// <summary>
    ///     Internal plugin version
    /// </summary>
    public static readonly Version Version =
        Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    
    // Instantiated
    private readonly ServiceProvider _services;
    
    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        var services = new ServiceCollection();

        // Services
        services.AddSingleton<ActionQueueService>();
        services.AddSingleton<CommandLockoutService>();
        services.AddSingleton<EmoteService>();
        services.AddSingleton<FriendsListService>();
        services.AddSingleton<IdentityService>();
        services.AddSingleton<LogService>();
        services.AddSingleton<NetworkService>();
        services.AddSingleton<PauseService>();
        services.AddSingleton<SpiralService>();
        services.AddSingleton<TipService>();
        services.AddSingleton<ViewService>();
        services.AddSingleton<WorldService>();
        
        // Services - Ipc
        services.AddSingleton<CustomizePlusIpc>();
        services.AddSingleton<GlamourerIpc>();
        services.AddSingleton<MoodlesIpc>();
        services.AddSingleton<PenumbraIpc>();
        
        // Managers
        services.AddSingleton<ChatCommandManager>();
        services.AddSingleton<ConnectivityManager>();
        services.AddSingleton<DependencyManager>();
        services.AddSingleton<ForwardedRequestManager>();
        services.AddSingleton<ModManager>();
        
        // Handlers
        services.AddSingleton<BodySwapHandler>();
        services.AddSingleton<EmoteHandler>();
        services.AddSingleton<HypnosisHandler>();
        services.AddSingleton<MoodlesHandler>();
        services.AddSingleton<SpeakHandler>();
        services.AddSingleton<SyncOnlineStatusHandler>();
        services.AddSingleton<SyncPermissionsHandler>();
        services.AddSingleton<TransformHandler>();
        services.AddSingleton<TwinningHandler>();
        services.AddSingleton<CustomizePlusHandler>();
        services.AddSingleton<NetworkHandler>(); // Aggregate handler
        
        // Ui - Components
        services.AddSingleton<FriendsListComponentUi>();
        services.AddSingleton<NavigationBarComponentUi>();
        
        // Ui - View Controllers
        services.AddSingleton<BodySwapViewUiController>();
        services.AddSingleton<CustomizePlusViewUiController>();
        services.AddSingleton<EmoteViewUiController>();
        services.AddSingleton<FriendsViewUiController>();
        services.AddSingleton<HistoryViewUiController>();
        services.AddSingleton<HypnosisViewUiController>();
        services.AddSingleton<LoginViewUiController>();
        services.AddSingleton<MoodlesViewUiController>();
        services.AddSingleton<PauseViewUiController>();
        services.AddSingleton<SettingsViewUiController>();
        services.AddSingleton<SpeakViewUiController>();
        services.AddSingleton<StatusViewUiController>();
        services.AddSingleton<TransformationViewUiController>();
        services.AddSingleton<TwinningViewUiController>();
        
        // Ui - Views
        services.AddSingleton<BodySwapViewUi>();
        services.AddSingleton<CustomizePlusViewUi>();
        services.AddSingleton<EmoteViewUi>();
        services.AddSingleton<FriendsViewUi>();
        services.AddSingleton<HistoryViewUi>();
        services.AddSingleton<HypnosisViewUi>();
        services.AddSingleton<LoginViewUi>();
        services.AddSingleton<MoodlesViewUi>();
        services.AddSingleton<PauseViewUi>();
        services.AddSingleton<SettingsViewUi>();
        services.AddSingleton<SpeakViewUi>();
        services.AddSingleton<StatusViewUi>();
        services.AddSingleton<TransformationViewUi>();
        services.AddSingleton<TwinningViewUi>();
        
        // Ui - Windows
        services.AddSingleton<MainWindow>();
        services.AddSingleton<WindowManager>();
        
        // Build the dependency injection framework
        _services = services.BuildServiceProvider();
        
        // Services
        _services.GetRequiredService<ActionQueueService>();
        _services.GetRequiredService<SpiralService>();
        
        // Managers
        _services.GetRequiredService<ChatCommandManager>();
        _services.GetRequiredService<ConnectivityManager>();
        _services.GetRequiredService<DependencyManager>();
        
        // Handlers
        _services.GetRequiredService<NetworkHandler>();
        
        // Ui - Windows
        _services.GetRequiredService<WindowManager>();
        
        Task.Run(SharedUserInterfaces.InitializeFonts);
    }
    
    public void Dispose()
    {
        _services.Dispose();
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