using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Settings;

public partial class SettingsView : IDrawable
{
    private readonly ActionQueueService _actionQueueService;
    private readonly CharacterConfigurationService _characterConfigurationService;
    private readonly CustomizePlusService _customizePlusService;
    private readonly GlamourerService _glamourerService;
    private readonly GlobalSettingsService _globalSettingsService;
    private readonly HonorificService _honorificService;
    private readonly MoodlesService _moodlesService;
    private readonly NetworkService _networkService;
    private readonly PenumbraService _penumbraService;
    private readonly SecretsService _secretsService;
    private readonly SettingsService _settingsService;
    private readonly DtrManager _dtrManager;
    private readonly HypnosisManager _hypnosisManager;
    
    public SettingsView(
        ActionQueueService actionQueueService, 
        CharacterConfigurationService characterConfigurationService,
        CustomizePlusService customizePlusService,
        GlamourerService glamourerService, 
        GlobalSettingsService globalSettingsService,
        HonorificService honorificService,
        MoodlesService moodlesService, 
        NetworkService networkService,
        PenumbraService penumbraService, 
        SecretsService secretsService,
        SettingsService settingsService,
        DtrManager dtrManager,
        HypnosisManager hypnosisManager)
    {
        _actionQueueService = actionQueueService;
        _characterConfigurationService = characterConfigurationService;
        _customizePlusService = customizePlusService;
        _glamourerService = glamourerService;
        _globalSettingsService = globalSettingsService;
        _honorificService = honorificService;
        _moodlesService = moodlesService;
        _networkService = networkService;
        _penumbraService = penumbraService;
        _secretsService = secretsService;
        _settingsService = settingsService;
        _dtrManager = dtrManager;
        _hypnosisManager = hypnosisManager;
    }
}