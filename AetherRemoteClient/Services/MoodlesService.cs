using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Domain.Moodles;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Moodles;
using Dalamud.Plugin.Ipc;
using MoodlesStatusInfo = (
    int Version,
    System.Guid Guid,
    int IconId,
    string Title,
    string Description,
    string CustomVfxPath,
    long ExpireTicks,
    AetherRemoteCommon.Domain.Moodles.MoodleType Type,
    int Stacks,
    int StackSteps,
    uint Modifiers,
    System.Guid ChainedStatus,
    AetherRemoteCommon.Domain.Moodles.MoodleChainTrigger ChainTrigger,
    string Applier,
    string Dispeller,
    bool Permanent);

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to Moodles
/// </summary>
public class MoodlesService : IExternalPlugin
{
    // Const
    private const int ExpectedMajor = 4;
    
    // Moodles Version
    private readonly ICallGateSubscriber<int> _version;
    
    // Moodles Status Managers
    private readonly ICallGateSubscriber<nint, string> _getStatusManager;
    private readonly ICallGateSubscriber<nint, string, object> _setStatusManager;
    
    // Moodles Statuses
    private readonly ICallGateSubscriber<List<MoodlesStatusInfo>> _listMoodles;
    private readonly ICallGateSubscriber<MoodlesStatusInfo, nint, object?> _applyMoodle;
    
    /// <summary>
    ///     Is Moodles available for use?
    /// </summary>
    public bool ApiAvailable;
        
    /// <summary>
    ///     <inheritdoc cref="IExternalPlugin.IpcReady"/>
    /// </summary>
    public event EventHandler? IpcReady;

    /// <summary>
    ///     <inheritdoc cref="MoodlesService"/>
    /// </summary>
    public MoodlesService()
    {
        // Moodles Version
        _version = Plugin.PluginInterface.GetIpcSubscriber<int>("Moodles.Version");
        
        // Moodles Status Managers
        _getStatusManager = Plugin.PluginInterface.GetIpcSubscriber<nint, string>("Moodles.GetStatusManagerByPtrV2");
        _setStatusManager = Plugin.PluginInterface.GetIpcSubscriber<nint, string, object>("Moodles.SetStatusManagerByPtrV2");
        
        // Moodles Statuses
        _listMoodles = Plugin.PluginInterface.GetIpcSubscriber<List<MoodlesStatusInfo>>("Moodles.GetStatusInfoListV2");
        _applyMoodle = Plugin.PluginInterface.GetIpcSubscriber<MoodlesStatusInfo, nint, object?>("Moodles.AddOrUpdateMoodleByDataByPtrV2");
    }
    
    /// <summary>
    ///     Tests for availability for Moodles
    /// </summary>
    public async Task<bool> TestIpcAvailability()
    {
        // Set everything to disabled state
        ApiAvailable = false;

        try
        {
            // Invoke Api
            var version = await DalamudUtilities.RunOnFramework(() => _version.InvokeFunc()).ConfigureAwait(false);
        
            // Test for proper versioning
            if (version < ExpectedMajor)
                return false;

            // Mark as ready
            ApiAvailable = true;
            IpcReady?.Invoke(this, EventArgs.Empty);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[MoodlesService.TestIpcAvailability] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Gets the Moodles Status Manager for a provided character
    /// </summary>
    public async Task<string?> GetStatusManager(nint address)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[MoodlesService.GetStatusManager] Api not available");
            return null;
        }

        try
        {
            return await DalamudUtilities.RunOnFramework(() => _getStatusManager.InvokeFunc(address)).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[MoodlesService.GetStatusManager] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Sets the Moodles Status Manager for a provided character
    /// </summary>
    public async Task<bool> SetStatusManager(nint address, string status)
    { 
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[MoodlesService.SetStatusManager] Api not available");
            return false;
        }

        try
        {
            await DalamudUtilities.RunOnFramework(() => _setStatusManager.InvokeAction(address, status)).ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[MoodlesService.SetStatusManager] {e}");
            return false;
        }
    }

    /// <summary>
    ///     Gets all the client's moodles
    /// </summary>
    public async Task<List<Moodle>?> GetMoodles()
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[MoodlesService.GetMoodles] Api not available");
            return null;
        }
        
        try
        {
            var moodles = await DalamudUtilities.RunOnFramework(() => _listMoodles.InvokeFunc()).ConfigureAwait(false);
            return moodles.Select(ConvertStatusInfoToMoodle).ToList();
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[MoodlesService.GetMoodles] {e}");
            return null;
        }
    }

    /// <summary>
    ///     Applies a Moodle to the client
    /// </summary>
    public async Task<bool> ApplyMoodle(MoodleInfo moodle)
    {
        if (ApiAvailable is false)
        {
            Plugin.Log.Warning("[MoodlesService.ApplyMoodle] Api not available");
            return false;
        }
        
        try
        {
            // Get the local player's pointer address
            if (await DalamudUtilities.RunOnFramework(() => Plugin.ObjectTable.LocalPlayer?.Address).ConfigureAwait(false) is not { } pointer)
                return false;

            // Call Moodles
            await DalamudUtilities.RunOnFramework(() => _applyMoodle.InvokeAction(ConvertMoodleToStatusInfo(moodle), pointer));
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[MoodlesService.ApplyMoodle] {e}");
            return false;
        }
    }
    
    private static MoodlesStatusInfo ConvertMoodleToStatusInfo(MoodleInfo info)
    {
        return new MoodlesStatusInfo
        {
            Version = info.Version,
            Guid = info.Guid,
            IconId = info.IconId,
            Title = info.Title,
            Description = info.Description,
            CustomVfxPath = info.CustomVfxPath,
            ExpireTicks = info.ExpireTicks,
            Type = info.Type,
            Stacks = info.Stacks,
            StackSteps = info.StackSteps,
            Modifiers = info.Modifiers,
            ChainedStatus = info.ChainedStatus,
            ChainTrigger = info.ChainTrigger,
            Applier = info.Applier,
            Dispeller = info.Dispeller,
            Permanent = info.Permanent
        };
    }

    private static Moodle ConvertStatusInfoToMoodle(MoodlesStatusInfo info)
    {
        var time = TimeSpan.FromMilliseconds(info.ExpireTicks < 0 ? 0 : info.ExpireTicks);
        
        return new Moodle
        {
            Info = new MoodleInfo
            {
                Version = info.Version,
                Guid = info.Guid,
                IconId = info.IconId,
                Title = info.Title,
                Description = info.Description,
                CustomVfxPath = info.CustomVfxPath,
                ExpireTicks = info.ExpireTicks,
                Type = info.Type,
                Stacks = info.Stacks,
                StackSteps = info.StackSteps,
                Modifiers = info.Modifiers,
                ChainedStatus = info.ChainedStatus,
                ChainTrigger = info.ChainTrigger,
                Applier = info.Applier,
                Dispeller = info.Dispeller,
                Permanent = info.Permanent
            },
                
            PrettyTitle = RemoveTagsFromTitle(info.Title),
            PrettyDescription = RemoveTagsFromTitle(info.Description, true),
            PrettyExpiration = $"{time.Days}d, {time.Hours}h, {time.Minutes}m, {time.Seconds}s"
        };
    }
    
    // TODO: This should not exist here
    /// <summary>
    ///     Removes all text between a [ and a ] including the brackets themselves
    /// </summary>
    public static string RemoveTagsFromTitle(string title, bool withNewLines = false)
    {
        // Resulting string
        var cleanTitle = new StringBuilder();
        
        // A variable to mark when we're inside a tag
        var cleaning = false;

        // Iterate over all the characters
        foreach (var character in title)
        {
            // Skip beginning bracket and mark that we are now cleaning a tag
            if (character is '[')
            {
                cleaning = true;
                continue;
            }

            // Skip ending bracket and mark that we are no longer cleaning a tag
            if (character is ']')
            {
                cleaning = false;
                continue;
            }
            
            // If we're cleaning, ignore the character
            if (cleaning)
                continue;
            
            // Skip new lines
            if (!withNewLines && character is '\n' or '\r')
            {
                cleanTitle.Append(' ');
                continue;
            }
            
            // Append the character otherwise
            cleanTitle.Append(character);
        }
        
        // Return resulting string
        return cleanTitle.ToString();
    }
}