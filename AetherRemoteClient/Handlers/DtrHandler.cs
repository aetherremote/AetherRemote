using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AetherRemoteClient.Handlers;

/// <summary>
///     TODO: Teehee :)
/// </summary>
public class DtrHandler : IDisposable
{
    // Const
    private const string AetherRemoteDtrId = "AetherRemoteDtr";

    // Injected
    private readonly ViewService _viewService;
    private readonly NetworkService _networkService;
    private readonly LoginManager _loginManager;
    private readonly StatusManager _statusManager;
    
    /// <summary>
    ///     Event fired when the Dtr bar is clicked
    /// </summary>
    public event Action? DtrClicked;
    
    /// <summary>
    ///     <inheritdoc cref="DtrHandler"/>
    /// </summary>
    public DtrHandler(ViewService viewService, NetworkService networkService, LoginManager loginManager, StatusManager statusManager)
    {
        _viewService = viewService;
        _networkService = networkService;
        _loginManager = loginManager;
        _statusManager = statusManager;

        _statusManager.StatusChanged += UpdateDtrBar;
        
        _networkService.Connected += UpdateDtrBarAsync;
        if (_networkService.CurrentlyConnected)
            UpdateDtrBarAsync();
        
        _networkService.Disconnected += UpdateDtrBarAsync;
        if (_networkService.CurrentlyConnected is false)
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
        if (Plugin.Configuration.ShowOnDtrBar is false)
            return;
        
        BuildDtrBar(_networkService.CurrentlyConnected, _statusManager.GetStatusCount());
    }

    /// <summary>
    ///     Removes the AR Dtr entry
    /// </summary>
    public static void RemoveDtrBar()
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
                if (_statusManager.CustomizePlus is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You have a Customize+ profile applied to you"));
                }
                
                if (_statusManager.GlamourerPenumbra is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You have an altered appearance or collection"));
                }
                
                if (_statusManager.Honorific is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You have an honorific applied to you"));
                }
                
                if (_statusManager.Hypnosis is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You are being hypnotized"));
                }
                
                if (_statusManager.Possession is not null)
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
        _statusManager.StatusChanged -= UpdateDtrBar;
        _networkService.Connected -= UpdateDtrBarAsync;
        _networkService.Disconnected -= UpdateDtrBarAsync;
        _loginManager.LoginFinished -= UpdateDtrBar;
        GC.SuppressFinalize(this);
    }
}