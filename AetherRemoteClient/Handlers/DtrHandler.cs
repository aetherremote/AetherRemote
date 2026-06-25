using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Style;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AetherRemoteClient.Handlers;

/// <summary>
///     Handles Dtr events and updates to display things like connectivity status
/// </summary>
public class DtrHandler : IDisposable
{
    // Const
    private const string AetherRemoteDtrId = "AetherRemoteDtr";

    // Injected
    private readonly NetworkService _networkService;
    private readonly SettingsService _settingsService;
    private readonly StatusService _statusService;
    private readonly ViewService _viewService;
    private readonly LoginManager _loginManager;
    
    /// <summary>
    ///     Event fired when the Dtr bar is clicked
    /// </summary>
    public event Action? DtrClicked;
    
    /// <summary>
    ///     <inheritdoc cref="DtrHandler"/>
    /// </summary>
    public DtrHandler(
        NetworkService networkService,
        SettingsService settingsService,
        ViewService viewService, 
        LoginManager loginManager, 
        StatusService statusService)
    {
        _networkService = networkService;
        _settingsService = settingsService;
        _viewService = viewService;
        _loginManager = loginManager;
        _statusService = statusService;

        _statusService.StatusChanged += UpdateDtrBar;
        
        _networkService.Connected += UpdateDtrBarAsync;
        if (_networkService.State is ConnectionState.Connected)
            UpdateDtrBarAsync();
        
        _networkService.Disconnected += UpdateDtrBarAsync;
        if (_networkService.State is not ConnectionState.Connected)
            UpdateDtrBarAsync();
        
        _loginManager.LoginFinished += UpdateDtrBar;
        if (_loginManager.HasLoginFinished)
            UpdateDtrBar();
    }
    
    /// <summary>
    ///     Updates the Dtr bar with information from the network service and status managers
    /// </summary>
    public void UpdateDtrBar()
    {
        if (_settingsService.ShowDtrBar is false)
            return;
        
        BuildDtrBar(_networkService.State is ConnectionState.Connected, _statusService.GetStatusCount());
    }

    /// <summary>
    ///     Removes the AR Dtr entry
    /// </summary>
    public void RemoveDtrBar()
    {
        Plugin.DtrBar.Remove(AetherRemoteDtrId);
    }

    /// <summary>
    ///     <inheritdoc cref="UpdateDtrBar"/>
    /// </summary>
    private Task UpdateDtrBarAsync()
    {
        UpdateDtrBar();
        return Task.CompletedTask;
    }
    
    private void BuildDtrBar(bool online, uint statusCount)
    {
        var title = new SeStringBuilder();
        if (online is false)
            title.AddUiGlow(AetherRemoteColors.TextColorRed);
        title.AddText(" AR");
        if (statusCount > 0)
            title.AddText(string.Concat('(', statusCount, ')'));
        if (online is false)
            title.AddUiGlowOff();
        
        var entry = Plugin.DtrBar.Get(AetherRemoteDtrId);
        entry.Text = title.Build();

        var tooltip = new SeStringBuilder();
        tooltip.AddText(string.Concat("Aether Remote Version ", Plugin.Version));
        if (online)
        {
            tooltip.AddUiGlow(AetherRemoteColors.TextColorGreen);
            tooltip.AddText(string.Concat(" Connected"));
            tooltip.AddUiGlowOff();
            if (statusCount > 0)
            {
                if (_statusService.CustomizePlus is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You have a Customize+ profile applied to you"));
                }
                
                if (_statusService.GlamourerPenumbra is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You have an altered appearance or collection"));
                }
                
                if (_statusService.Honorific is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You have an honorific applied to you"));
                }
                
                if (_statusService.Hypnosis is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You are being hypnotized"));
                }
                
                if (_statusService.Possession is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You are being possessed"));
                }
            }
        }
        else
        {
            tooltip.AddUiGlow(AetherRemoteColors.TextColorRed);
            tooltip.AddText(string.Concat(" Disconnected"));
            tooltip.AddUiGlowOff();
        }
        
        entry.Tooltip = tooltip.Build();
        
        // Open the main window and go to the status page if online, otherwise the login page
        entry.OnClick = _ =>
        {
            if (online)
            {
                _viewService.CurrentView = statusCount is 0 ? View.Home : View.Status;
            }
            else
            {
                _viewService.CurrentView = online ? View.Status : View.Login;
            }

            DtrClicked?.Invoke();
        };
        
        // Lastly, mark it as shown
        entry.Shown = true;
    }

    public void Dispose()
    {
        RemoveDtrBar();
        _statusService.StatusChanged -= UpdateDtrBar;
        _networkService.Connected -= UpdateDtrBarAsync;
        _networkService.Disconnected -= UpdateDtrBarAsync;
        _loginManager.LoginFinished -= UpdateDtrBar;
        GC.SuppressFinalize(this);
    }
}