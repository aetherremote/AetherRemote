using System;
using System.Numerics;
using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Hooks;
using AetherRemoteClient.Managers;
using Dalamud.Game.ClientState.Objects.Enums;
using Newtonsoft.Json;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUiController()
{
    public async void Debug()
    {
        try
        {
            
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"{e}");
        }
    }
    
    public async void Debug2()
    {
        try
        {
            
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"{e}");
        }
    }
}