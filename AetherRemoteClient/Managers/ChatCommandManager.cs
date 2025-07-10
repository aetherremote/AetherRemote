using System;
using System.Collections.Generic;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI;
using AetherRemoteClient.Utils;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AetherRemoteClient.Managers;

public class ChatCommandManager : IDisposable
{
    private const string CommandNameShort = "/ar";
    private const string CommandNameFull = "/aetherremote";
    private const string CommandNameOld = "/remote";

    private const string StopArg = "stop";
    private const string SafeMode = "safemode";
    private const string SafeWord = "safeword";
    
    // Injected
    private readonly ActionQueueService _actionQueueService;
    private readonly IdentityService _identityService;
    private readonly PermanentLockService _permanentLockService;
    private readonly SpiralService _spiralService;
    private readonly MainWindow _mainWindow;
    
    public ChatCommandManager(ActionQueueService actionQueueService, IdentityService identityService, PermanentLockService permanentLockService, SpiralService spiralService, MainWindow mainWindow)
    {
        _actionQueueService = actionQueueService;
        _identityService = identityService;
        _permanentLockService = permanentLockService;
        _spiralService = spiralService;
        _mainWindow = mainWindow;
        
        Plugin.CommandManager.AddHandler(CommandNameShort, new CommandInfo(OnCommand)
        {
            HelpMessage = $"""
                           Opens the primary plugin window
                           /ar {StopArg} - Stops all current spirals
                           /ar {SafeMode} - Put the plugin into safe mode
                           /ar {SafeWord} - Put the plugin into safe mode
                           """
        });
        
        Plugin.CommandManager.AddHandler(CommandNameFull, new CommandInfo(OnCommand)
        {
            ShowInHelp = false
        });
        
        Plugin.CommandManager.AddHandler(CommandNameOld, new CommandInfo(OnCommand)
        {
            ShowInHelp = false
        });
    }
    
    private void OnCommand(string command, string args)
    {
        if (args == string.Empty)
        {
            _mainWindow.IsOpen = true;
            return;
        }

        var payloads = new List<Payload>();
        switch (args)
        {
            case StopArg:
                _spiralService.StopCurrentSpiral();
                payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorPurple));
                payloads.Add(new TextPayload("[AetherRemote] "));
                payloads.Add(UIForegroundPayload.UIForegroundOff);
                payloads.Add( new TextPayload("Stopped current spirals"));
                break;
            
            case SafeMode:
            case SafeWord:
                // Unlock any permanent transformations
                _permanentLockService.CurrentLock = null;
                Plugin.Configuration.PermanentTransformations.Remove(_identityService.Character.FullName);
                Plugin.Configuration.Save();
                
                // Stop any spirals
                _spiralService.StopCurrentSpiral();
                
                // Clear pending chat commands
                _actionQueueService.Clear();
                
                // Enter safe mode
                Plugin.Configuration.SafeMode = true;
                Plugin.Configuration.Save();
                
                payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorPurple));
                payloads.Add(new TextPayload("[AetherRemote] "));
                payloads.Add(UIForegroundPayload.UIForegroundOff);
                payloads.Add(new TextPayload("Plugin is now in "));
                payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorGreen));
                payloads.Add(new TextPayload("safe mode"));
                payloads.Add(UIForegroundPayload.UIForegroundOff);
                break;
            
            default:
                payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorPurple));
                payloads.Add(new TextPayload("[AetherRemote] "));
                payloads.Add(UIForegroundPayload.UIForegroundOff);
                payloads.Add(new TextPayload($"Unknown argument \"{args}\""));
                break;
        }
        
        if (payloads.Count > 0)
            Plugin.ChatGui.Print(new SeString(payloads));
    }

    public void Dispose()
    {
        Plugin.CommandManager.RemoveHandler(CommandNameShort);
        Plugin.CommandManager.RemoveHandler(CommandNameFull);
        Plugin.CommandManager.RemoveHandler(CommandNameOld);
        
        GC.SuppressFinalize(this);
    }
}