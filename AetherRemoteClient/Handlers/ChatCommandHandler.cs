using System;
using System.Collections.Generic;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI;
using AetherRemoteClient.Utils;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AetherRemoteClient.Handlers;

public class ChatCommandHandler : IDisposable
{
    private const string CommandNameShort = "/ar";
    private const string CommandNameFull = "/aetherremote";
    private const string CommandNameOld = "/remote";

    private const string StopArg = "stop";
    private const string SafeMode = "safemode";
    private const string SafeWord = "safeword";
    private const string Unpossess = "unpossess";
    
    // Injected
    private readonly ActionQueueService _actionQueueService;
    private readonly HypnosisManager _hypnosisManager;
    private readonly PossessionManager _possessionManager;
    private readonly PermanentTransformationHandler _permanentTransformationHandler;
    private readonly MainWindow _mainWindow;
    
    public ChatCommandHandler(
        ActionQueueService actionQueueService,
        HypnosisManager hypnosisManager,
        PossessionManager possessionManager,
        PermanentTransformationHandler permanentTransformationHandler,
        MainWindow mainWindow)
    {
        _actionQueueService = actionQueueService;
        _hypnosisManager = hypnosisManager;
        _possessionManager = possessionManager;
        _permanentTransformationHandler = permanentTransformationHandler;
        _mainWindow = mainWindow;
        
        Plugin.CommandManager.AddHandler(CommandNameShort, new CommandInfo(OnCommand)
        {
            HelpMessage = $"""
                           Opens the primary plugin window
                           /ar {StopArg} - Stops all current spirals
                           /ar {SafeMode} - Put the plugin into safe mode
                           /ar {SafeWord} - Put the plugin into safe mode
                           /ar {Unpossess} - Stops possessing or being possessed
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
    
    private async void OnCommand(string command, string args)
    {
        try
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
                    // Stop any spirals
                    _hypnosisManager.Wake();
                    payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorPurple));
                    payloads.Add(new TextPayload("[AetherRemote] "));
                    payloads.Add(UIForegroundPayload.UIForegroundOff);
                    payloads.Add( new TextPayload("Stopped current spirals"));
                    break;
                
                case SafeMode:
                case SafeWord:
                    // Remove permanent transformations
                    _permanentTransformationHandler.ForceClearPermanentTransformation();
                    
                    // Stop any spirals
                    _hypnosisManager.Wake();
                    
                    // Stops possessing or being possessed
                    await _possessionManager.EndAllParanormalActivity(true).ConfigureAwait(false);
                    
                    // Clear pending chat commands
                    _actionQueueService.Clear();
                    
                    // Enter safe mode
                    Plugin.Configuration.SafeMode = true;
                    await Plugin.Configuration.Save().ConfigureAwait(false);
                    
                    payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorPurple));
                    payloads.Add(new TextPayload("[AetherRemote] "));
                    payloads.Add(UIForegroundPayload.UIForegroundOff);
                    payloads.Add(new TextPayload("Plugin is now in "));
                    payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorGreen));
                    payloads.Add(new TextPayload("safe mode"));
                    payloads.Add(UIForegroundPayload.UIForegroundOff);
                    break;
                
                case Unpossess:
                    await _possessionManager.EndAllParanormalActivity(true).ConfigureAwait(false);
                    
                    payloads.Add(new UIForegroundPayload(AetherRemoteStyle.TextColorPurple));
                    payloads.Add(new TextPayload("[AetherRemote] "));
                    payloads.Add(UIForegroundPayload.UIForegroundOff);
                    payloads.Add(new TextPayload("Stopped all possession activities."));
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
        catch (Exception e)
        {
            Plugin.Log.Error($"[ChatCommandHandler.OnCommand] {e}");
        }
    }

    public void Dispose()
    {
        Plugin.CommandManager.RemoveHandler(CommandNameShort);
        Plugin.CommandManager.RemoveHandler(CommandNameFull);
        Plugin.CommandManager.RemoveHandler(CommandNameOld);
        
        GC.SuppressFinalize(this);
    }
}