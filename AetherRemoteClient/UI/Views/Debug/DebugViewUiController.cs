using System;
using System.Numerics;
using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteClient.Dependencies.Honorific.Services;
using Dalamud.Game.ClientState.Objects.Enums;
using Newtonsoft.Json;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUiController(HonorificService honorific)
{
    public async void Debug()
    {
        try
        {
            //
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
            if (Plugin.ObjectTable.LocalPlayer?.ObjectIndex is not { } index)
                return;
            
            await honorific.ClearCharacterTitle(index);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"{e}");
        }
    }
}