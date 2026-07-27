using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Hypnosis;
using AetherRemoteClient.Domain.Hypnosis.Components;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Network.Domain;
using AetherRemoteCommon.Network.Domain.Payloads;
using Dalamud.Bindings.ImGui;
using Newtonsoft.Json;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public partial class HypnosisView
{
    // Const
    private static readonly Vector2 DefaultPreviewWindowSize = new(400);
    private const int SpiralRefreshCooldownInMilliseconds = 300;
    private const int TextRefreshCooldownInMilliseconds = 300;

    // Configuration values
    private int _spiralArms = HypnosisSpiralRenderer.DefaultSpiralArms;
    private int _spiralTurns = HypnosisSpiralRenderer.DefaultSpiralTurns;
    private int _spiralCurve = HypnosisSpiralRenderer.DefaultSpiralCurve;
    private int _spiralThickness = HypnosisSpiralRenderer.DefaultSpiralThickness;
    private int _spiralSpeed = HypnosisSpiralRenderer.DefaultSpiralSpeed;
    private int _spiralDirection = (int)HypnosisSpiralRenderer.DefaultSpiralDirection;
    private Vector4 _spiralColor = ImGui.ColorConvertU32ToFloat4(HypnosisSpiralRenderer.DefaultSpiralColor);

    // Text Configuration
    private int _textDelay = HypnosisTextRenderer.DefaultTextDelayInMilliseconds / 1000;
    private int _textDuration = HypnosisTextRenderer.DefaultTextDurationInMilliseconds / 1000;
    private int _textMode = (int)HypnosisTextRenderer.DefaultHypnosisTextMode;
    private string _textWords = string.Empty;
    private Vector4 _textColor = ImGui.ColorConvertU32ToFloat4(HypnosisTextRenderer.DefaultTextColor);

    // Save Load Spirals
    private string _saveLoadSpiralSearchText = string.Empty;
    private readonly ListFilter<string> _saveLoadSpiralFileOptionsListFilter;
    private readonly List<string> _saveLoadSpiralFileOptions = [];

    // Refresh Timers
    private readonly Timer _spiralRefreshCooldown = new(SpiralRefreshCooldownInMilliseconds);
    private readonly Timer _textRefreshCooldown = new(TextRefreshCooldownInMilliseconds);

    // Preview Window Size
    private Vector2 _previousPreviewWindowSize = DefaultPreviewWindowSize;

    // Renderer
    private readonly HypnosisRenderer _hypnosisRenderer = new();

    // Renders the spiral and text
    private void RenderSpiralAndText(ImDrawListPtr draw, Vector2 screenSize, Vector2 screenPosition) => _hypnosisRenderer.Render(draw, screenSize, screenPosition);

    /// <summary>
    ///     Begin an internal countdown so we're not initiating spiral refreshes every frame
    /// </summary>
    private void BeginSpiralRefreshTimer()
    {
        _spiralRefreshCooldown.Stop();
        _spiralRefreshCooldown.Start();
    }

    private async void OnRefreshSpiral(object? sender, ElapsedEventArgs e)
    {
        try
        {
            await _hypnosisRenderer.Spiral.SetSpiral(_spiralArms, _spiralTurns, _spiralCurve, _spiralThickness);
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"[HypnosisViewUiController.OnRefreshSpiral] {exception}");
        }
    }

    /// <summary>
    ///     Begin an internal countdown so we're not initiating text refreshes every frame
    /// </summary>
    private void BeginTextRefreshTimer()
    {
        _textRefreshCooldown.Stop();
        _textRefreshCooldown.Start();
    }

    private async void OnRefreshText(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var lines = _textWords.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
            await _hypnosisRenderer.Text.SetText(lines, _previousPreviewWindowSize);
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"[HypnosisViewUiController.OnRefreshText] {exception}");
        }
    }

    /// <summary>
    ///     Attempts to save the hypnosis profile name in the save / load text input box
    /// </summary>
    private async void SaveHypnosisProfileToDisk()
    {
        try
        {
            // Only attempt if there is any text in the input
            if (_saveLoadSpiralSearchText.Length is 0)
                return;

            // Get the hypnosis data from the ui
            var hypnosisData = GetHypnosisDataFromUi();

            // Convert to a profile
            var profile = new HypnosisProfile
            {
                Name = _saveLoadSpiralSearchText,
                Data = hypnosisData
            };

            // Save the profile
            await HypnosisSaveService.SaveHypnosisProfile(profile).ConfigureAwait(false);

            // Notification
            NotificationHelper.Success("Successfully saved", string.Empty);
            
            // Refresh the search options for our list of filenames
            RefreshSavedSpiralFileNames();
        }
        catch (Exception e)
        {
            NotificationHelper.Error("Unable to save", e.Message);
            Plugin.Log.Error($"[HypnosisViewUiController.SaveHypnosisProfileToDisk] {e}");
        }
    }

    /// <summary>
    ///     Attempts to load the hypnosis profile name in the save / load text input box
    /// </summary>
    private async void LoadHypnosisProfileFromDisk()
    {
        try
        {
            // Only attempt if there is any text in the input
            if (_saveLoadSpiralSearchText.Length is 0)
                return;

            // Only proceed if the load was successful
            if (await HypnosisSaveService.LoadHypnosisProfile(_saveLoadSpiralSearchText).ConfigureAwait(false) is not { } hypnosisProfile)
                return;

            // Set the text to display the name of what you loaded
            _saveLoadSpiralSearchText = hypnosisProfile.Name;

            // Set the Ui elements to match
            SetUiFromHypnosisData(hypnosisProfile.Data);
            
            // Notification
            NotificationHelper.Success("Successfully loaded", string.Empty);

            // Sync everything to the renderer
            await SyncHypnosisDataToHypnosisRenderer(hypnosisProfile.Data);
        }
        catch (Exception e)
        {
            NotificationHelper.Error("Unable to load", e.Message);
            Plugin.Log.Error($"[HypnosisViewUiController.LoadHypnosisProfileFromDisk] {e}");
        }
    }

    /// <summary>
    ///     Attempts to delete the hypnosis profile name in the save / load text input box
    /// </summary>
    private async void DeleteHypnosisProfileFromDisk()
    {
        try
        {
            // Only attempt if there is any text in the input
            if (_saveLoadSpiralSearchText.Length is 0)
                return;

            // Attempt to delete the configuration
            await HypnosisSaveService.DeleteHypnosisProfile(_saveLoadSpiralSearchText).ConfigureAwait(false);
            
            // Clear text
            _saveLoadSpiralSearchText = string.Empty;
            
            // Notification
            NotificationHelper.Success("Deleted successfully", string.Empty);
            
            // Refresh the search options for our list of filenames
            RefreshSavedSpiralFileNames();
        }
        catch (Exception e)
        {
            NotificationHelper.Error("Unable to delete", e.Message);
            Plugin.Log.Error($"[HypnosisViewUiController.LoadHypnosisProfileFromDisk] {e}");
        }
    }

    /// <summary>
    ///     Exports the current hypnosis data to the clipboard
    /// </summary>
    private async void ExportToClipboard()
    {
        try
        {
            // Get the hypnosis data from the ui
            var hypnosisData = GetHypnosisDataFromUi();

            // Convert to JSON
            var json = await Task.Run(() => JsonConvert.SerializeObject(hypnosisData)).ConfigureAwait(false);

            // Copy to clipboard
            ImGui.SetClipboardText(json);
            
            // Notification
            NotificationHelper.Success("Successfully exported to clipboard", string.Empty);
        }
        catch (Exception e)
        {
            NotificationHelper.Error("Unable to export to clipboard", e.Message);
            Plugin.Log.Error($"[HypnosisViewUiController.ExportToClipboard] {e}");
        }
    }

    /// <summary>
    ///     Imports the clipboard data and attempts to set the Ui to match
    /// </summary>
    private async void ImportFromClipboard()
    {
        try
        {
            // Get whatever is in the clipboard
            var json = ImGui.GetClipboardText();

            // Convert to object
            if (await Task.Run(() => JsonConvert.DeserializeObject<HypnosisData>(json)).ConfigureAwait(false) is not { } hypnosisData)
                return;

            // Set the Ui elements to match
            SetUiFromHypnosisData(hypnosisData);

            // Notification
            NotificationHelper.Success("Successfully imported from clipboard", string.Empty);
            
            // Sync everything to the renderer
            await SyncHypnosisDataToHypnosisRenderer(hypnosisData);
        }
        catch (Exception e)
        {
            NotificationHelper.Error("Unable to import from clipboard", e.Message);
            Plugin.Log.Error($"[HypnosisViewUiController.ImportFromClipboard] {e}");
        }
    }

    // Set individual hypnosis spiral attributes
    private void SetSpeed() => _hypnosisRenderer.Spiral.SetSpeed(_spiralSpeed);
    private void SetDirection() => _hypnosisRenderer.Spiral.SetDirection((HypnosisSpiralDirection)_spiralDirection);
    private void SetColorSpiral() => _hypnosisRenderer.Spiral.SetColor(_spiralColor);

    // Set individual hypnosis text attributes
    private void SetDelay() => _hypnosisRenderer.Text.SetDelay(_textDelay * 1000);
    private void SetDuration() => _hypnosisRenderer.Text.SetDuration(_textDuration * 1000);
    private void SetMode() => _hypnosisRenderer.Text.SetMode((HypnosisTextMode)_textMode);
    private void SetColorText() => _hypnosisRenderer.Text.SetColor(_textColor);

    /// <summary>
    ///     Sends a hypnosis request
    /// </summary>
    private async Task SendHypnosis()
    {
        var targets = _selectionManager.GetSelectedFriendCodes();
        if (targets.Count is 0)
            return;
        
        _commandLockoutService.Lock();
        var payload = new HypnosisPayload(GetHypnosisDataFromUi());
        await _networkRequestManager.Send<HypnosisPayload, NoPayload>(targets, HubMethod.Hypnosis, payload).ConfigureAwait(false);
    }

    /// <summary>
    ///     Sends a hypnosis request specifically to stop a spiral
    /// </summary>
    private async Task StopHypnosis()
    {
        var targets = _selectionManager.GetSelectedFriendCodes();
        if (targets.Count is 0)
            return;
        
        _commandLockoutService.Lock();
        var payload = new HypnosisStopPayload();
        await _networkRequestManager.Send<HypnosisStopPayload, NoPayload>(targets, HubMethod.HypnosisStop, payload).ConfigureAwait(false);
    }

    /// <summary>
    ///     Load all the filenames in the saved hypnosis profiles folder
    /// </summary>
    private async void RefreshSavedSpiralFileNames()
    {
        try
        {
            // Only proceed if the folder exists
            if (Directory.Exists(HypnosisSaveService.HypnosisFolderPath) is false)
                return;
        
            // Clear original list
            _saveLoadSpiralFileOptions.Clear();

            // Get all files in the folder
            var filePaths = await Task.Run(() => Directory.GetFiles(HypnosisSaveService.HypnosisFolderPath)).ConfigureAwait(false);
        
            // Add only the filename without extension to the list
            foreach (var file in filePaths)
                _saveLoadSpiralFileOptions.Add(Path.GetFileNameWithoutExtension(file));
            
            // Refresh the current search terms
            _saveLoadSpiralFileOptionsListFilter.UpdateSearchTerm(_saveLoadSpiralSearchText);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HypnosisViewUiController.RefreshSavedSpiralFileNames] {e}");
        }
    }

    /// <summary>
    ///     Converts the local UI elements into HypnosisData format
    /// </summary>
    private HypnosisData GetHypnosisDataFromUi()
    {
        var lines = _textWords.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        return new HypnosisData
        {
            SpiralArms = _spiralArms,
            SpiralTurns = _spiralTurns,
            SpiralCurve = _spiralCurve,
            SpiralThickness = _spiralThickness,
            SpiralSpeed = _spiralSpeed,
            SpiralDirection = (HypnosisSpiralDirection)_spiralDirection,
            SpiralColor = ImGui.ColorConvertFloat4ToU32(_spiralColor),

            TextDuration = _textDuration,
            TextDelay = _textDelay,
            TextMode = (HypnosisTextMode)_textMode,
            TextColor = ImGui.ColorConvertFloat4ToU32(_textColor),
            TextWords = lines
        };
    }

    /// <summary>
    ///     Sets the local UI elements to the values in HypnosisData
    /// </summary>
    private void SetUiFromHypnosisData(HypnosisData data)
    {
        _spiralArms = data.SpiralArms;
        _spiralTurns = data.SpiralTurns;
        _spiralCurve = data.SpiralCurve;
        _spiralThickness = data.SpiralThickness;
        _spiralSpeed = data.SpiralSpeed;
        _spiralDirection = (int)data.SpiralDirection;
        _spiralColor = ImGui.ColorConvertU32ToFloat4(data.SpiralColor);

        _textDuration = data.TextDuration;
        _textDelay = data.TextDelay;
        _textMode = (int)data.TextMode;
        _textColor = ImGui.ColorConvertU32ToFloat4(data.TextColor);
        _textWords = string.Join(Environment.NewLine, data.TextWords);
    }

    /// <summary>
    ///     Syncs everything from the Ui to the HypnosisRenderer
    /// </summary>
    private async Task SyncHypnosisDataToHypnosisRenderer(HypnosisData data)
    {
        await _hypnosisRenderer.SetRendererFromHypnosisData(data, _previousPreviewWindowSize).ConfigureAwait(false);
    }
    

}