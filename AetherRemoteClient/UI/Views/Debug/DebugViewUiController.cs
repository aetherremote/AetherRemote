using System;
using System.Numerics;
using AetherRemoteClient.Dependencies.CustomizePlus.Domain;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Honorific.Domain;
using AetherRemoteClient.Dependencies.Honorific.Services;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Hooks;
using AetherRemoteClient.Managers;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.Command;
using Newtonsoft.Json;

namespace AetherRemoteClient.UI.Views.Debug;

public class DebugViewUiController(CustomizePlusService c)
{
    public FolderNode<Profile>? _node;
    
    public async void Debug()
    {
        try
        {
            Plugin.CommandManager.ProcessCommand("/honorific force set title Hello");
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