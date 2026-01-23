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
            //
            possessionManager.TryBecomePossessed();
            possessionManager.SetMovementDirection(1.0f, 0, 1.0f, 0);
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
            possessionManager.SetMovementDirection(0, 0,0 ,0);
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"{e}");
        }
    }
}