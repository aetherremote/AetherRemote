using AetherRemoteClient.Domain.Logger;
using AetherRemoteCommon.Domain.CommonGlamourerApplyType;
using Dalamud.Plugin;
using Dalamud.Plugin.Ipc;
using Dalamud.Plugin.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AetherRemoteClient.Accessors.Glamourer;

public class GlamourerAccessor : IDisposable
{
    private readonly AetherRemoteLogger logger;
    private readonly ICallGateSubscriber<(int, int)> glamourerApiVersions;
    private readonly ICallGateSubscriber<string, string?> glamourerGetAllCustomization;
    private readonly ICallGateSubscriber<string, string, object> glamourerApplyAll;
    private readonly ICallGateSubscriber<string, string, object> glamourerApplyOnlyEquipment;
    private readonly ICallGateSubscriber<string, string, object> glamourerApplyOnlyCustomization;

    public bool IsGlamourerInstalled { get; private set; }

    private readonly CancellationTokenSource source = new();
    private readonly TimeSpan checkGlamourerApiInterval = TimeSpan.FromSeconds(15);

    public GlamourerAccessor(AetherRemoteLogger logger, IDalamudPluginInterface pluginInterface)
    {
        this.logger = logger;

        glamourerApiVersions = pluginInterface.GetIpcSubscriber<(int, int)>("Glamourer.ApiVersions");
        glamourerGetAllCustomization = pluginInterface.GetIpcSubscriber<string, string?>("Glamourer.GetAllCustomization");
        glamourerApplyAll = pluginInterface.GetIpcSubscriber<string, string, object>("Glamourer.ApplyAll");
        glamourerApplyOnlyEquipment = pluginInterface.GetIpcSubscriber<string, string, object>("Glamourer.ApplyOnlyEquipment");
        glamourerApplyOnlyCustomization = pluginInterface.GetIpcSubscriber<string, string, object>("Glamourer.ApplyOnlyCustomization");

        PeriodicCheckGlamourerApi(() => { 
            IsGlamourerInstalled = CheckGlamourerInstalled();
        }, source.Token);
    }

    /// <summary>
    /// Apply the glamourer design to a specified character.
    /// </summary>
    /// <param name="characterName"></param>
    /// <param name="glamourerData"></param>
    /// <param name="applyType"></param>
    /// <returns>If applying the design was successful</returns>
    public bool ApplyDesign(string characterName, string glamourerData, GlamourerApplyType applyType)
    {
        if (!IsGlamourerInstalled) return false;

        var operation = applyType switch
        {
            GlamourerApplyType.CustomizationAndEquipment => glamourerApplyAll,
            GlamourerApplyType.Customization => glamourerApplyOnlyCustomization,
            GlamourerApplyType.Equipment => glamourerApplyOnlyEquipment,
            _ => glamourerApplyAll
        };

        try
        {
            operation.InvokeAction(glamourerData, characterName);
            return true;
        }
        catch
        {
            logger.Warning($"Glamourer::{operation} - Unable to apply glamourer data.");
            return false;
        }
    }

    /// <summary>
    /// Get a character's glamourer data.
    /// </summary>
    /// <param name="characterName"></param>
    /// <returns>The character's glamourer data as a string.</returns>
    public string? GetCustomization(string characterName)
    {
        if (!IsGlamourerInstalled)
        {
            logger.Warning("Glamourer::GetAllCustomization - Glamourer not installed.");
            return null;
        }

        try
        {
            return glamourerGetAllCustomization.InvokeFunc(characterName);
        }
        catch
        {
            logger.Warning("Glamourer::GetAllCustomization - Unable to get glamourer customization details.");
            return null;
        }
    }

    /// <summary>
    /// Translates booleans into glamourer apply types.
    /// </summary>
    /// <returns></returns>
    public static GlamourerApplyType ConvertBoolsToApplyType(bool applyCustomization, bool applyEquipment)
    {
        return (applyEquipment, applyCustomization) switch
        {
            (true, true) => GlamourerApplyType.CustomizationAndEquipment,
            (false, false) => GlamourerApplyType.CustomizationAndEquipment,
            (true, false) => GlamourerApplyType.Equipment,
            (false, true) => GlamourerApplyType.Customization,
        };
    }

    private void PeriodicCheckGlamourerApi(Action action, CancellationToken token)
    {
        if (action == null) return;
        Task.Run(async () =>
        {
            while (!token.IsCancellationRequested)
            {
                action();
                await Task.Delay(checkGlamourerApiInterval, token);
            }
        }, token);
    }

    private bool CheckGlamourerInstalled()
    {
        var isGlamourerInstalled = false;
        try
        {
            var version = glamourerApiVersions.InvokeFunc();
            if (version.Item1 == 0 && version.Item2 >= 1)
            {
                isGlamourerInstalled = true;
            }

            return isGlamourerInstalled;
        }
        catch
        {
            return isGlamourerInstalled;
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        source.Cancel();
    }
}
