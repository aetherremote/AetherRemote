using System.Linq;
using System.Threading.Tasks;
using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Log;
using AetherRemoteClient.Providers;
using AetherRemoteClient.Uncategorized;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using AetherRemoteCommon.Domain.Network.Commands;

namespace AetherRemoteClient.UI.Tabs.Managers;

public class TransformationManager(
    ClientDataManager clientDataManager,
    CommandLockoutManager commandLockoutManager,
    GlamourerAccessor glamourerAccessor,
    HistoryLogProvider historyLogProvider,
    NetworkProvider networkProvider)
{
    // Const
    private const GlamourerApplyFlag CustomizationFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization;
    private const GlamourerApplyFlag EquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Equipment;
    private const GlamourerApplyFlag CustomizationAndEquipmentFlags = GlamourerApplyFlag.Once | GlamourerApplyFlag.Customization | GlamourerApplyFlag.Equipment;
    
    // Variables - Glamourer
    public string GlamourerData = "";
    public bool ApplyCustomization = true;
    public bool ApplyEquipment = true;
    
    public async Task Become()
    {
        if (GlamourerData.Length == 0) return;

        var glamourerApplyFlags = GlamourerApplyFlag.Once 
                                  | (ApplyCustomization ? GlamourerApplyFlag.Customization : 0) 
                                  | (ApplyEquipment ? GlamourerApplyFlag.Equipment : 0);

        if (glamourerApplyFlags == GlamourerApplyFlag.Once) 
            glamourerApplyFlags = CustomizationAndEquipmentFlags;
        
        commandLockoutManager.Lock();

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();

        var request = new TransformRequest(targets, GlamourerData, glamourerApplyFlags);
        var result = await networkProvider.InvokeCommand<TransformRequest, TransformResponse>(Network.Commands.Transform, request).ConfigureAwait(false);
        if (result.Success)
        {
            var targetNames = string.Join(", ", targets);
            var logMessage = glamourerApplyFlags switch
            {
                CustomizationFlags => $"You issued {targetNames} to change their appearance",
                EquipmentFlags => $"You issued {targetNames} to change their outfit",
                CustomizationAndEquipmentFlags => $"You issued {targetNames} to change their outfit, and appearance",
                _ => $"You issued {targetNames} to change"
            };

            Plugin.Log.Information(logMessage);
            historyLogProvider.LogHistoryGlamourer(logMessage, GlamourerData);

            // Reset glamourer data
            GlamourerData = string.Empty;
        }
        else
        {
            Plugin.Log.Warning($"Issuing transform command unsuccessful: {result.Message}");
        }
    }
    
    public async Task Revert(RevertType revertType)
    {
        commandLockoutManager.Lock();

        var targets = clientDataManager.TargetManager.Targets.Keys.ToList();

        var request = new RevertRequest(targets, revertType);
        var result = await networkProvider.InvokeCommand<RevertRequest, RevertResponse>(Network.Commands.Revert, request);
        if (result.Success)
        {
            var targetNames = string.Join(", ", targets);
            var logMessage = revertType switch
            {
                RevertType.Automation => $"You issued {targetNames} to revert to their automations",
                RevertType.Game => $"You issued {targetNames} to revert to game",
                _ => $"You issued {targetNames} to revert"
            };

            Plugin.Log.Information(logMessage);
            historyLogProvider.LogHistory(logMessage);
        }
        else
        {
            Plugin.Log.Warning($"Issuing revert command unsuccessful: {result.Message}");
        }
    }
    
    public async Task CopyMyGlamourerDataAsync()
    {
        // TODO: Logging
        if (GameObjectManager.LocalPlayerExists() is false)
            return;

        GlamourerData = await glamourerAccessor.GetDesignAsync().ConfigureAwait(false) ?? string.Empty;
    }

    public async Task CopyTargetGlamourerData()
    {
        // TODO: Logging
        var target = GameObjectManager.GetTargetPlayerObjectIndex();
        if (target is null)
            return;

        GlamourerData = await glamourerAccessor.GetDesignAsync(target.Value).ConfigureAwait(false) ?? string.Empty;
    }
}