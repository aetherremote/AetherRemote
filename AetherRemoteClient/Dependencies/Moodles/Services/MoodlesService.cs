using MoodlesStatusInfo = (
    System.Guid GUID,
    int IconID,
    string Title,
    string Description,
    AetherRemoteCommon.Dependencies.Moodles.Enums.MoodleType Type,
    string Applier,
    bool Dispelable,
    int Stacks,
    bool Persistent,
    int Days,
    int Hours,
    int Minutes,
    int Seconds,
    bool NoExpire,
    bool AsPermanent,
    System.Guid StatusOnDispell,
    string CustomVFXPath,
    bool StackOnReapply,
    int StacksIncOnReapply);

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AetherRemoteClient.Dependencies.Moodles.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteCommon.Dependencies.Moodles.Domain;
using Dalamud.Plugin.Ipc;

namespace AetherRemoteClient.Dependencies.Moodles.Services;

/// <summary>
///     Provides access to Moodles
/// </summary>
public class MoodlesService : IExternalPlugin
{
    // Const
    private const int ExpectedMajor = 3;
    
    // Moodles Version
    private readonly ICallGateSubscriber<int> _version;
    
    // Moodles Status Managers
    private readonly ICallGateSubscriber<nint, string> _getStatusManager;
    private readonly ICallGateSubscriber<nint, string, object> _setStatusManager;
    
    // Moodles Statuses
    private readonly ICallGateSubscriber<List<MoodlesStatusInfo>> _listMoodles;
    private readonly ICallGateProvider<MoodlesStatusInfo, object?> _applyMoodle;
    
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
        _version = Plugin.PluginInterface.GetIpcSubscriber<int>("Moodles.Version");
        
        _getStatusManager = Plugin.PluginInterface.GetIpcSubscriber<nint, string>("Moodles.GetStatusManagerByPtr");
        _setStatusManager = Plugin.PluginInterface.GetIpcSubscriber<nint, string, object>("Moodles.SetStatusManagerByPtr");
        
        _listMoodles = Plugin.PluginInterface.GetIpcSubscriber<List<MoodlesStatusInfo>>("Moodles.GetStatusInfoListV2");
        _applyMoodle = Plugin.PluginInterface.GetIpcProvider<MoodlesStatusInfo, object?>("GagSpeak.ApplyStatusInfo");
    }
    
    /// <summary>
    ///     Tests for availability for Moodles
    /// </summary>
    public async Task<bool> TestIpcAvailability()
    {
        // Set everything to disabled state
        ApiAvailable = false;
        
        // Invoke Api
        var version = await Plugin.RunOnFrameworkSafely(() => _version.InvokeFunc()).ConfigureAwait(false);
        
        // Test for proper versioning
        if (version < ExpectedMajor)
            return false;

        // Mark as ready
        ApiAvailable = true;
        IpcReady?.Invoke(this, EventArgs.Empty);
        return true;
    }

    /// <summary>
    ///     Gets the Moodles Status Manager for a provided character
    /// </summary>
    public async Task<string?> GetStatusManager(nint address)
    {
        return await ExecuteOnThread(() => _getStatusManager.InvokeFunc(address)).ConfigureAwait(false);
    }

    /// <summary>
    ///     Sets the Moodles Status Manager for a provided character
    /// </summary>
    public async Task<bool> SetStatusManager(nint address, string status)
    { 
        return await ExecuteOnThread(() => _setStatusManager.InvokeAction(address, status)).ConfigureAwait(false);
    }

    /// <summary>
    ///     Gets all the client's moodles
    /// </summary>
    /// <returns></returns>
    public async Task<List<Moodle>> GetMoodles()
    {
        var results = new List<Moodle>();
        foreach (var statusInfo in await ExecuteOnThread(() => _listMoodles.InvokeFunc()).ConfigureAwait(false) ?? [])
            results.Add(ConvertStatusInfoToMoodle(statusInfo));

        return results;
    }

    /// <summary>
    ///     Applies a Moodle to the client
    /// </summary>
    public async Task<bool> ApplyMoodle(MoodleInfo moodle)
    {
        return await ExecuteOnThread(() => _applyMoodle.SendMessage(ConvertMoodleToStatusInfo(moodle))).ConfigureAwait(false);
    }
    
    /// <summary>
    ///     Executes a Moodles command on the main thread
    /// </summary>
    private async Task<bool> ExecuteOnThread(Action action)
    {
        if (!ApiAvailable)
        {
            Plugin.Log.Warning("[MoodlesService] Api not available");
            return false;
        }

        try
        {
            await Plugin.Framework.RunOnFrameworkThread(action).ConfigureAwait(false);
            return true;
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[MoodlesService] An expected error occurred, {e}");
            return false;
        }
    }

    /// <summary>
    ///     Executes a Moodles command on the main thread
    /// </summary>
    private async Task<T?> ExecuteOnThread<T>(Func<T> function)
    {
        if (!ApiAvailable)
        {
            Plugin.Log.Warning("[MoodlesService] Api not available");
            return default;
        }

        try
        {
            return await Plugin.Framework.RunOnFrameworkThread(function).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"[MoodlesService] An expected error occurred, {e}");
            return default;
        }
    }
    
    private static MoodlesStatusInfo ConvertMoodleToStatusInfo(MoodleInfo info)
    {
        return new MoodlesStatusInfo
        {
            GUID = info.Guid,
            IconID = info.IconId,
            Title = info.Title,
            Description = info.Description,
            Type = info.Type,
            Applier = info.Applier,
            Dispelable = info.Dispellable,
            Stacks = info.Stacks,
            Persistent = info.Persistent,
            Days = info.Days,
            Hours = info.Hours,
            Minutes = info.Minutes,
            Seconds = info.Seconds,
            NoExpire = info.NoExpire,
            AsPermanent = info.AsPermanent,
            StatusOnDispell = info.StatusOnRemoval,
            CustomVFXPath = info.CustomVfxPath,
            StackOnReapply = info.StackOnReapply,
            StacksIncOnReapply = info.StacksIncOnReapply
        };
    }

    private static Moodle ConvertStatusInfoToMoodle(MoodlesStatusInfo info)
    {
        return new Moodle
        {
            Info = new MoodleInfo
            {
                Guid = info.GUID,
                IconId = info.IconID,
                Title = info.Title,
                Description = info.Description,
                Type = info.Type,
                Applier = info.Applier,
                Dispellable = info.Dispelable,
                Stacks = info.Stacks,
                Persistent = info.Persistent,
                Days = info.Days,
                Hours = info.Hours,
                Minutes = info.Minutes,
                Seconds = info.Seconds,
                NoExpire = info.NoExpire,
                AsPermanent = info.AsPermanent,
                StatusOnRemoval = info.StatusOnDispell,
                CustomVfxPath = info.CustomVFXPath,
                StackOnReapply = info.StackOnReapply,
                StacksIncOnReapply = info.StacksIncOnReapply,
            },
                
            PrettyTitle = RemoveTagsFromTitle(info.Title),
            PrettyDescription = RemoveTagsFromTitle(info.Description, true)
        };
    }
    
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