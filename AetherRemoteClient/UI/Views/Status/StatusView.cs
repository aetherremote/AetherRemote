using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Status;

public partial class StatusView : IDrawable
{
    private readonly CustomizePlusService _customizePlusService;
    private readonly GlamourerService _glamourerService;
    private readonly HonorificService _honorificService;
    private readonly PenumbraService _penumbraService;
    private readonly StatusService _statusService;
    private readonly HypnosisManager _hypnosisManager;
    private readonly CharacterTransformationManager _characterTransformationManager;
    private readonly PossessionManager _possessionManager;
    
    public StatusView(
        CustomizePlusService customizePlusService,
        GlamourerService glamourerService,
        HonorificService honorificService,
        PenumbraService penumbraService,
        StatusService statusService,
        HypnosisManager hypnosisManager,
        CharacterTransformationManager characterTransformationManager,
        PossessionManager possessionManager)
    {
        _customizePlusService = customizePlusService;
        _glamourerService = glamourerService;
        _honorificService = honorificService;
        _penumbraService = penumbraService;
        _statusService = statusService;
        _hypnosisManager = hypnosisManager;
        _characterTransformationManager = characterTransformationManager;
        _possessionManager = possessionManager;
    }
}