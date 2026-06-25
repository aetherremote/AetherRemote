using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Style;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AetherRemoteClient.Managers;

/// <summary>
///     Handles Dtr events and updates to display things like connectivity status
/// </summary>
public class DtrManager(NetworkService networkService, StatusService statusService, ViewService viewService) : IDisposable
{
    // Const
    private const string AetherRemoteDtrId = "AetherRemoteDtr";
    
    /// <summary>
    ///     Event fired when the Dtr bar is clicked
    /// </summary>
    public event Action? DtrClicked;

    /// <summary>
    ///     Enables the AR Dtr Bar entry
    /// </summary>
    public void EnableDtrBar()
    {
        BuildDtrBar();
        
        statusService.StatusChanged += BuildDtrBar;
        networkService.Connected += BuildDtrBarAsync;
        networkService.Disconnected += BuildDtrBarAsync;
    }

    /// <summary>
    ///     Disables the AR Dtr Bar entry 
    /// </summary>
    public void DisableDtrBar()
    {
        statusService.StatusChanged -= BuildDtrBar;
        networkService.Connected -= BuildDtrBarAsync;
        networkService.Disconnected -= BuildDtrBarAsync;
        
        Plugin.DtrBar.Remove(AetherRemoteDtrId);
    }

    private void BuildDtrBar() => _ = BuildDtrBarAsync().ConfigureAwait(false);
    private Task BuildDtrBarAsync()
    {
        var online = networkService.State is ConnectionState.Connected;
        var statusCount = statusService.GetStatusCount();
        
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
                if (statusService.CustomizePlus is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You have a Customize+ profile applied to you"));
                }
                
                if (statusService.GlamourerPenumbra is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You have an altered appearance or collection"));
                }
                
                if (statusService.Honorific is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You have an honorific applied to you"));
                }
                
                if (statusService.Hypnosis is not null)
                {
                    tooltip.Add(new NewLinePayload());
                    tooltip.AddText(string.Concat("You are being hypnotized"));
                }
                
                if (statusService.Possession is not null)
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
                viewService.CurrentView = statusCount is 0 ? View.Home : View.Status;
            }
            else
            {
                viewService.CurrentView = online ? View.Status : View.Login;
            }

            DtrClicked?.Invoke();
        };
        
        // Lastly, mark it as shown
        entry.Shown = true;
        
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        DisableDtrBar();
        GC.SuppressFinalize(this);
    }
}