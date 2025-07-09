using System;
using System.Threading.Tasks;
using AetherRemoteClient.Domain;

namespace AetherRemoteClient.Services;

/// <summary>
///     Manages a flavor aspect of body swapping and twinning
/// </summary>
public class IdentityService : IDisposable
{
    /// <summary>
    ///     Your friend code
    /// </summary>
    public string FriendCode = "Unknown Friend Code";

    /// <summary>
    ///     The local character you logged in as
    /// </summary>
    public LocalCharacter Character = new("Unknown", "Unknown");
    
    /// <summary>
    ///     Name of your current in-game identity you are assuming
    /// </summary>
    public string Identity = "Unknown Identity";
    
    /// <summary>
    ///     <inheritdoc cref="IdentityService"/>
    /// </summary>
    public IdentityService()
    {
        Plugin.ClientState.Login += OnLogin;
        Plugin.ClientState.Logout += OnLogout;
        //Identity = Plugin.ClientState.LocalPlayer?.Name.TextValue ?? Identity;
    }

    /// <summary>
    ///     Reset identity to in-game character
    /// </summary>
    public async Task SetIdentityToCurrentCharacter()
    {
        Identity = await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer?.Name.TextValue).ConfigureAwait(false) ?? "Unknown Identity";
    }
    
    private async void OnLogin()
    {
        try
        {
            Identity = await Plugin.RunOnFramework(() => Plugin.ClientState.LocalPlayer?.Name.TextValue).ConfigureAwait(false) ?? "Unknown Identity";
        }
        catch (Exception)
        {
            // Ignored
        }
    }
    
    private void OnLogout(int _, int __)
    {
        Identity = "Unknown Identity";
    }

    public void Dispose()
    {
        Plugin.ClientState.Login -= OnLogin;
        Plugin.ClientState.Logout -= OnLogout;
        GC.SuppressFinalize(this);
    }
}