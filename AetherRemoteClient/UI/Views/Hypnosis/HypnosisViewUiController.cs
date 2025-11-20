using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Hypnosis;
using AetherRemoteClient.Domain.Hypnosis.Components;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using AetherRemoteCommon.Domain.Network.Hypnosis;
using Dalamud.Bindings.ImGui;
using Newtonsoft.Json;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public class HypnosisViewUiController : IDisposable
{
    // Const
    public static readonly Vector2 DefaultPreviewWindowSize = new(400);
    private const int SpiralRefreshCooldownInMilliseconds = 300;
    private const int TextRefreshCooldownInMilliseconds = 300;

    // Injected
    private readonly NetworkService _networkService;
    private readonly SelectionManager _selectionManager;

    // Configuration values
    public int SpiralArms = HypnosisSpiralRenderer.DefaultSpiralArms;
    public int SpiralTurns = HypnosisSpiralRenderer.DefaultSpiralTurns;
    public int SpiralCurve = HypnosisSpiralRenderer.DefaultSpiralCurve;
    public int SpiralThickness = HypnosisSpiralRenderer.DefaultSpiralThickness;
    public int SpiralSpeed = HypnosisSpiralRenderer.DefaultSpiralSpeed;
    public int SpiralDirection = (int)HypnosisSpiralRenderer.DefaultSpiralDirection;
    public Vector4 SpiralColor = ImGui.ColorConvertU32ToFloat4(HypnosisSpiralRenderer.DefaultSpiralColor);

    // Text Configuration
    public int TextDelay = HypnosisTextRenderer.DefaultTextDelayInMilliseconds / 1000;
    public int TextDuration = HypnosisTextRenderer.DefaultTextDurationInMilliseconds / 1000;
    public int TextMode = (int)HypnosisTextRenderer.DefaultHypnosisTextMode;
    public string TextWords = string.Empty;
    public Vector4 TextColor = ImGui.ColorConvertU32ToFloat4(HypnosisTextRenderer.DefaultTextColor);

    // Save Load Spirals
    public string SaveLoadSpiralSearchText = string.Empty;
    public readonly ListFilter<string> SaveLoadSpiralFileOptionsListFilter;
    private readonly List<string> _saveLoadSpiralFileOptions = [];

    // Refresh Timers
    private readonly Timer _spiralRefreshCooldown = new(SpiralRefreshCooldownInMilliseconds);
    private readonly Timer _textRefreshCooldown = new(TextRefreshCooldownInMilliseconds);

    // Preview Window Size
    public Vector2 PreviousPreviewWindowSize = DefaultPreviewWindowSize;

    // Renderer
    private readonly HypnosisRenderer _hypnosisRenderer = new();

    /// <summary>
    ///     <inheritdoc cref="HypnosisViewUiController"/>
    /// </summary>
    public HypnosisViewUiController(NetworkService networkService, SelectionManager selectionManager)
    {
        _networkService = networkService;
        _selectionManager = selectionManager;

        _spiralRefreshCooldown.AutoReset = false;
        _spiralRefreshCooldown.Enabled = false;
        _spiralRefreshCooldown.Elapsed += OnRefreshSpiral;

        _textRefreshCooldown.AutoReset = false;
        _textRefreshCooldown.Enabled = false;
        _textRefreshCooldown.Elapsed += OnRefreshText;

        SaveLoadSpiralFileOptionsListFilter = new ListFilter<string>(_saveLoadSpiralFileOptions,
            (spiralName, searchTerm) => spiralName.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

        RefreshSavedSpiralFileNames();
        
        BeginSpiralRefreshTimer();
    }

    // Renders the spiral and text
    public void RenderSpiralAndText(ImDrawListPtr draw, Vector2 screenSize, Vector2 screenPosition) =>
        _hypnosisRenderer.Render(draw, screenSize, screenPosition);

    /// <summary>
    ///     Begin an internal countdown so we're not initiating spiral refreshes every frame
    /// </summary>
    public void BeginSpiralRefreshTimer()
    {
        _spiralRefreshCooldown.Stop();
        _spiralRefreshCooldown.Start();
    }

    private async void OnRefreshSpiral(object? sender, ElapsedEventArgs e)
    {
        try
        {
            await _hypnosisRenderer.Spiral.SetSpiral(SpiralArms, SpiralTurns, SpiralCurve, SpiralThickness);
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"[HypnosisViewUiController.OnRefreshSpiral] {exception}");
        }
    }

    /// <summary>
    ///     Begin an internal countdown so we're not initiating text refreshes every frame
    /// </summary>
    public void BeginTextRefreshTimer()
    {
        _textRefreshCooldown.Stop();
        _textRefreshCooldown.Start();
    }

    private async void OnRefreshText(object? sender, ElapsedEventArgs e)
    {
        try
        {
            var lines = TextWords.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
            await _hypnosisRenderer.Text.SetText(lines, PreviousPreviewWindowSize);
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"[HypnosisViewUiController.OnRefreshText] {exception}");
        }
    }

    /// <summary>
    ///     Attempts to save the hypnosis profile name in the save / load text input box
    /// </summary>
    public async void SaveHypnosisProfileToDisk()
    {
        try
        {
            // Only attempt if there is any text in the input
            if (SaveLoadSpiralSearchText.Length is 0)
                return;

            // Get the hypnosis data from the ui
            var hypnosisData = GetHypnosisDataFromUi();

            // Convert to a profile
            var profile = new HypnosisProfile
            {
                Name = SaveLoadSpiralSearchText,
                Data = hypnosisData
            };

            // Save the profile
            await ConfigurationService.SaveHypnosisProfile(profile).ConfigureAwait(false);

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
    public async void LoadHypnosisProfileFromDisk()
    {
        try
        {
            // Only attempt if there is any text in the input
            if (SaveLoadSpiralSearchText.Length is 0)
                return;

            // Only proceed if the load was successful
            if (await ConfigurationService.LoadHypnosisProfile(SaveLoadSpiralSearchText).ConfigureAwait(false) is not { } hypnosisProfile)
                return;

            // Set the text to display the name of what you loaded
            SaveLoadSpiralSearchText = hypnosisProfile.Name;

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
    public async void DeleteHypnosisProfileFromDisk()
    {
        try
        {
            // Only attempt if there is any text in the input
            if (SaveLoadSpiralSearchText.Length is 0)
                return;

            // Attempt to delete the configuration
            await ConfigurationService.DeleteHypnosisProfile(SaveLoadSpiralSearchText).ConfigureAwait(false);
            
            // Clear text
            SaveLoadSpiralSearchText = string.Empty;
            
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
    public async void ExportToClipboard()
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
    public async void ImportFromClipboard()
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
    public void SetSpeed() => _hypnosisRenderer.Spiral.SetSpeed(SpiralSpeed);
    public void SetDirection() => _hypnosisRenderer.Spiral.SetDirection((HypnosisSpiralDirection)SpiralDirection);
    public void SetColorSpiral() => _hypnosisRenderer.Spiral.SetColor(SpiralColor);

    // Set individual hypnosis text attributes
    public void SetDelay() => _hypnosisRenderer.Text.SetDelay(TextDelay * 1000);
    public void SetDuration() => _hypnosisRenderer.Text.SetDuration(TextDuration * 1000);
    public void SetMode() => _hypnosisRenderer.Text.SetMode((HypnosisTextMode)TextMode);
    public void SetColorText() => _hypnosisRenderer.Text.SetColor(TextColor);

    /// <summary>
    ///     Sends a hypnosis request
    /// </summary>
    public async void SendHypnosis()
    {
        try
        {
            var data = GetHypnosisDataFromUi();
            var request = new HypnosisRequest(_selectionManager.GetSelectedFriendCodes(), data, false);
            var response = await _networkService.InvokeAsync<ActionResponse>(HubMethod.Hypnosis, request).ConfigureAwait(false);
        
            ActionResponseParser.Parse("Hypnosis", response);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HypnosisViewUiController.SendHypnosis] {e}");
        }
    }

    /// <summary>
    ///     Sends a hypnosis request specifically to stop a spiral
    /// </summary>
    public async void StopHypnosis()
    {
        try
        {
            var request = new HypnosisRequest(_selectionManager.GetSelectedFriendCodes(), new HypnosisData(), true);
            var response = await _networkService.InvokeAsync<ActionResponse>(HubMethod.Hypnosis, request).ConfigureAwait(false);
            
            ActionResponseParser.Parse("Hypnosis Stop", response);
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[HypnosisViewUiController.StopHypnosis] {e}");
        }
    }

    /// <summary>
    ///     Load all the filenames in the saved hypnosis profiles folder
    /// </summary>
    private async void RefreshSavedSpiralFileNames()
    {
        try
        {
            // Only proceed if the folder exists
            if (Directory.Exists(ConfigurationService.HypnosisFolderPath) is false)
                return;
        
            // Clear original list
            _saveLoadSpiralFileOptions.Clear();

            // Get all files in the folder
            var filePaths = await Task.Run(() => Directory.GetFiles(ConfigurationService.HypnosisFolderPath)).ConfigureAwait(false);
        
            // Add only the filename without extension to the list
            foreach (var file in filePaths)
                _saveLoadSpiralFileOptions.Add(Path.GetFileNameWithoutExtension(file));
            
            // Refresh the current search terms
            SaveLoadSpiralFileOptionsListFilter.UpdateSearchTerm(SaveLoadSpiralSearchText);
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
        var lines = TextWords.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
        return new HypnosisData
        {
            SpiralArms = SpiralArms,
            SpiralTurns = SpiralTurns,
            SpiralCurve = SpiralCurve,
            SpiralThickness = SpiralThickness,
            SpiralSpeed = SpiralSpeed,
            SpiralDirection = (HypnosisSpiralDirection)SpiralDirection,
            SpiralColor = ImGui.ColorConvertFloat4ToU32(SpiralColor),

            TextDuration = TextDuration,
            TextDelay = TextDelay,
            TextMode = (HypnosisTextMode)TextMode,
            TextColor = ImGui.ColorConvertFloat4ToU32(TextColor),
            TextWords = lines
        };
    }

    /// <summary>
    ///     Sets the local UI elements to the values in HypnosisData
    /// </summary>
    private void SetUiFromHypnosisData(HypnosisData data)
    {
        SpiralArms = data.SpiralArms;
        SpiralTurns = data.SpiralTurns;
        SpiralCurve = data.SpiralCurve;
        SpiralThickness = data.SpiralThickness;
        SpiralSpeed = data.SpiralSpeed;
        SpiralDirection = (int)data.SpiralDirection;
        SpiralColor = ImGui.ColorConvertU32ToFloat4(data.SpiralColor);

        TextDuration = data.TextDuration;
        TextDelay = data.TextDelay;
        TextMode = (int)data.TextMode;
        TextColor = ImGui.ColorConvertU32ToFloat4(data.TextColor);
        TextWords = string.Join(Environment.NewLine, data.TextWords);
    }

    /// <summary>
    ///     Syncs everything from the Ui to the HypnosisRenderer
    /// </summary>
    private async Task SyncHypnosisDataToHypnosisRenderer(HypnosisData data)
    {
        await _hypnosisRenderer.SetRendererFromHypnosisData(data, PreviousPreviewWindowSize).ConfigureAwait(false);
    }
    
    public void Dispose()
    {
        // Dispose of the spiral timer
        _spiralRefreshCooldown.Elapsed += OnRefreshSpiral;
        _spiralRefreshCooldown.Dispose();
        
        // Dispose of the text timer
        _textRefreshCooldown.Elapsed -= OnRefreshText;
        _textRefreshCooldown.Dispose();
        
        // Dispose of the textures created in the hypnosis renderer
        _hypnosisRenderer.Dispose();
        GC.SuppressFinalize(this);
    }
}