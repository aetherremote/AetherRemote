using System;
using System.Numerics;
using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Managers;
using Dalamud.Game.ClientState.Objects.Enums;
using Newtonsoft.Json;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUiController(HonorificService honorific, PossessionManager  possessionManager)
{
    public async void Debug()
    {
        try
        {
            var h = (float)((Random.Shared.NextDouble() * 2.0 - 1.0) * Math.PI);
            var v = (float)((Random.Shared.NextDouble() * 2.0 - 1.0) * (85 * Math.PI / 180));
            var z = 1.5f + (float)Random.Shared.NextDouble() * (20f - 1.5f);
            
            //
            possessionManager.TryBecomePossessed();
            possessionManager.SetCameraDestination(h, v, z);
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