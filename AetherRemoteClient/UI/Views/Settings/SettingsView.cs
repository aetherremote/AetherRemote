using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Infrastructure.Database;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Services.Configuration;
using AetherRemoteClient.Services.Dependencies;

namespace AetherRemoteClient.UI.Views.Settings;

public partial class SettingsView : IView
{
    // IView property
    public View View => View.Settings;
    
    // Injected
    private readonly DatabaseInfrastructure _databaseInfrastructure;
    private readonly ActionQueueService _actionQueueService;
    private readonly ActiveSessionService _activeSessionService;
    private readonly ConfigurationService _configurationService;
    private readonly CustomizePlusService _customizePlusService;
    private readonly GlamourerService _glamourerService;
    private readonly HonorificService _honorificService;
    private readonly MoodlesService _moodlesService;
    private readonly PenumbraService _penumbraService;
    private readonly DtrManager _dtrManager;
    private readonly HypnosisManager _hypnosisManager;
    
    public SettingsView(
        DatabaseInfrastructure databaseInfrastructure,
        ActionQueueService actionQueueService, 
        ActiveSessionService activeSessionService,
        ConfigurationService configurationService,
        CustomizePlusService customizePlusService,
        GlamourerService glamourerService, 
        HonorificService honorificService,
        MoodlesService moodlesService, 
        PenumbraService penumbraService, 
        DtrManager dtrManager,
        HypnosisManager hypnosisManager)
    {
        _databaseInfrastructure = databaseInfrastructure;
        _actionQueueService = actionQueueService;
        _activeSessionService = activeSessionService;
        _configurationService = configurationService;
        _customizePlusService = customizePlusService;
        _glamourerService = glamourerService;
        _honorificService = honorificService;
        _moodlesService = moodlesService;
        _penumbraService = penumbraService;
        _dtrManager = dtrManager;
        _hypnosisManager = hypnosisManager;
    }
}