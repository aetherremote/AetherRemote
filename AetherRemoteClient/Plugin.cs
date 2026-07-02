using System;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AetherRemoteClient.Handlers;
using AetherRemoteClient.Handlers.Chat;
using AetherRemoteClient.Handlers.Network;
using AetherRemoteClient.Hooks;
using AetherRemoteClient.Infrastructure.Authentication;
using AetherRemoteClient.Infrastructure.Database;
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
public sealed class Plugin : IAsyncDalamudPlugin
{
    [PluginService] internal static IChatGui ChatGui                                { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState                        { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager                  { get; private set; } = null!;
    [PluginService] internal static IDalamudPluginInterface PluginInterface         { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager                        { get; private set; } = null!;
    [PluginService] internal static IDtrBar DtrBar                                  { get; private set; } = null!;
    [PluginService] internal static IFramework Framework                            { get; private set; } = null!;
    [PluginService] internal static IGameConfig GameConfig                          { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider        { get; private set; } = null!;
    [PluginService] internal static INotificationManager NotificationManager        { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable                        { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log                                  { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner                          { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager                    { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider                { get; private set; } = null!;

    /// <summary>
    ///     Internal plugin version
    /// </summary>
    public static readonly Version Version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
    
    // Instantiated
    private ServiceProvider? _services;
    
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        // Create a collection of services
        var services = new ServiceCollection();
        
        // Infrastructure
        services.AddSingleton<AuthenticationInfrastructure>();
        services.AddSingleton<DatabaseInfrastructure>();
        
        // Services
        services.AddSingleton<AccountService>();
        services.AddSingleton<ActionQueueService>();
        services.AddSingleton<ActiveSessionService>();
        services.AddSingleton<AgreementsService>();
        services.AddSingleton<CharacterConfigurationService>();
        services.AddSingleton<CommandLockoutService>();
        services.AddSingleton<EmoteService>();
        services.AddSingleton<FriendsListService>();
        services.AddSingleton<GameSettingsService>();
        services.AddSingleton<GlobalSettingsService>();
        services.AddSingleton<LegacyConfigurationImportService>();
        services.AddSingleton<LogService>();
        services.AddSingleton<NetworkService>();
        services.AddSingleton<NotesService>();
        services.AddSingleton<PauseService>();
        services.AddSingleton<SecretsService>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<StatusService>();
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
        services.AddSingleton<DtrManager>();
        services.AddSingleton<HypnosisManager>();
        services.AddSingleton<NetworkCommandManager>();
        services.AddSingleton<PossessionManager>();
        services.AddSingleton<SelectionManager>();
        
        // Handlers
        services.AddSingleton<ChatCommandHandler>();
        services.AddSingleton<GlamourerEventHandler>();
        services.AddSingleton<LoginHandler>();
        services.AddSingleton<NetworkHandler>();
        
        // Ui - Component Controllers
        services.AddSingleton<FriendsListComponentUiController>();
        
        // Ui - Components
        services.AddSingleton<FriendsListComponentUi>();
        services.AddSingleton<NavigationBarComponentUi>();
        
        // Ui - Views
        services.AddSingleton<CustomizePlusView>();
        services.AddSingleton<DebugView>();
        services.AddSingleton<EmoteView>();
        services.AddSingleton<FriendsView>();
        services.AddSingleton<HistoryView>();
        services.AddSingleton<HomeView>();
        services.AddSingleton<HonorificView>();
        services.AddSingleton<HypnosisView>();
        services.AddSingleton<LoginView>();
        services.AddSingleton<MoodlesView>();
        services.AddSingleton<PauseView>();
        services.AddSingleton<PossessionView>();
        services.AddSingleton<SettingsView>();
        services.AddSingleton<SpeakView>();
        services.AddSingleton<StatusView>();
        services.AddSingleton<TransformationsView>();
        
        // Ui - Windows
        services.AddSingleton<MainWindow>();
        services.AddSingleton<WindowManager>();
        
        // Build the dependency injection framework
        _services = services.BuildServiceProvider();
        
        // Upgrade legacy configuration files
        await _services.GetRequiredService<LegacyConfigurationImportService>().ScanForConfigurationsAndImport().ConfigureAwait(false);
        
        // Ui - Windows
        _services.GetRequiredService<WindowManager>();
        
        // Ui - Views
        _services.GetRequiredService<NavigationBarComponentUi>();           // Required to listen to log in / log out events
        _services.GetRequiredService<CustomizePlusView>();                  // Required to display UI elements when IPCs are loaded
        _services.GetRequiredService<HonorificView>();                      // Required to display UI elements when IPCs are loaded
        _services.GetRequiredService<LoginView>();                          // Required to display secret once character configuration loads
        _services.GetRequiredService<MoodlesView>();                        // Required to display UI elements when IPCs are loaded
        _services.GetRequiredService<TransformationsView>();                // Required to display UI elements when IPCs are loaded
        
        // Handlers
        _services.GetRequiredService<ChatCommandHandler>();
        _services.GetRequiredService<GlamourerEventHandler>();
        _services.GetRequiredService<LoginHandler>();
        _services.GetRequiredService<NetworkHandler>();
        
        // Managers
        _services.GetRequiredService<ConnectionManager>();
        _services.GetRequiredService<DependencyManager>();
        _services.GetRequiredService<DtrManager>();
        _services.GetRequiredService<HypnosisManager>();
        _services.GetRequiredService<PossessionManager>();
        
        // Services
        _services.GetRequiredService<ActionQueueService>();
        
        // TODO: Examine what options there are for throwing exceptions in this method
        // Async loading for required services. If any of these fail, the server should probably throw an exception...
        await _services.GetRequiredService<AgreementsService>().LoadAgreements().ConfigureAwait(false);
        await _services.GetRequiredService<GlobalSettingsService>().LoadGlobalSettings().ConfigureAwait(false);
        await _services.GetRequiredService<NotesService>().LoadNotes().ConfigureAwait(false);
        await _services.GetRequiredService<SecretsService>().LoadSecrets().ConfigureAwait(false);
        
        await SharedUserInterfaces.InitializeFonts().ConfigureAwait(false);
        
        ActionResponseParser.SanityCheck();
    }
    
    public async ValueTask DisposeAsync()
    {
        if (_services is null)
            return;
        
        await _services.DisposeAsync().ConfigureAwait(false);
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