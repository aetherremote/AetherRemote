using System;
using System.Collections.Generic;
using System.Text;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Glamourer.Services;
using AetherRemoteClient.Dependencies.Penumbra.Services;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Managers.Possession;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI;
using Dalamud.Game.Command;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;

namespace AetherRemoteClient.Handlers.Chat;

public partial class ChatCommandHandler : IDisposable
{
    private const string CommandNameShort = "/ar";
    private const string CommandNameFull = "/aetherremote";
    private const string CommandNameOld = "/remote";

    private const string StopArg = "stop";
    private const string SafeMode = "safemode";
    private const string SafeWord = "safeword";
    private const string Unpossess = "unpossess";
    private const string Emote = "emote";
    private const string Speak = "speak";
    private const string Customize = "customize";
    private const string Transform = "transform";
    
    // Injected
    private readonly ActionQueueService _actionQueueService;
    private readonly CustomizePlusService _customizePlusService;
    private readonly EmoteService _emoteService;
    private readonly GlamourerService _glamourerService;
    private readonly HypnosisManager _hypnosisManager;
    private readonly NetworkCommandManager _networkCommandManager;
    private readonly PossessionManager _possessionManager;
    private readonly PermanentTransformationHandler _permanentTransformationHandler;
    private readonly MainWindow _mainWindow;
    
    public ChatCommandHandler(
        ActionQueueService actionQueueService,
        CustomizePlusService customizePlusService,
        EmoteService emoteService,
        GlamourerService glamourerService,
        HypnosisManager hypnosisManager,
        NetworkCommandManager networkCommandManager,
        PossessionManager possessionManager,
        PermanentTransformationHandler permanentTransformationHandler,
        MainWindow mainWindow)
    {
        _actionQueueService = actionQueueService;
        _customizePlusService = customizePlusService;
        _emoteService =  emoteService;
        _glamourerService = glamourerService;
        _hypnosisManager = hypnosisManager;
        _networkCommandManager = networkCommandManager;
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

            var c = args.Split(" ");
            if (c.Length == 0)
                return;

            var co = c[0];

            var payloads = new List<Payload>();
            switch (co)
            {
                case StopArg:
                    // Stop any spirals
                    _hypnosisManager.Wake();
                    payloads.Add(new UIForegroundPayload(AetherRemoteColors.TextColorPurple));
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
                    
                    payloads.Add(new UIForegroundPayload(AetherRemoteColors.TextColorPurple));
                    payloads.Add(new TextPayload("[AetherRemote] "));
                    payloads.Add(UIForegroundPayload.UIForegroundOff);
                    payloads.Add(new TextPayload("Plugin is now in "));
                    payloads.Add(new UIForegroundPayload(AetherRemoteColors.TextColorGreen));
                    payloads.Add(new TextPayload("safe mode"));
                    payloads.Add(UIForegroundPayload.UIForegroundOff);
                    break;
                
                case Unpossess:
                    await _possessionManager.EndAllParanormalActivity(true).ConfigureAwait(false);
                    
                    payloads.Add(new UIForegroundPayload(AetherRemoteColors.TextColorPurple));
                    payloads.Add(new TextPayload("[AetherRemote] "));
                    payloads.Add(UIForegroundPayload.UIForegroundOff);
                    payloads.Add(new TextPayload("Stopped all possession activities."));
                    break;
                
                // Example: /ar customize [targets] [profile id] [additive]
                case Customize:
                    _ = HandleCustomize(args).ConfigureAwait(false);
                    break;
                
                // Example: /ar emote [targets] [emote] [display log message]
                case Emote:
                    _ = HandleEmote(args).ConfigureAwait(false);
                    break;
                
                // Example: /ar speak [targets] [channel] [message]
                case Speak:
                    _ = HandleSpeak(args).ConfigureAwait(false);
                    break;
                
                // Example: /ar transform [targets] [design name] [apply type]
                case Transform:
                    _ = HandleTransform(args).ConfigureAwait(false);
                    break;
                
                default:
                    payloads.Add(new UIForegroundPayload(AetherRemoteColors.TextColorPurple));
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

    private static string[] ExtractArguments(string input)
    {
        var tokens = new List<string>();
        var current = new StringBuilder();
        var inQuotes = false;

        foreach (var character in input)
        {
            if (character == '"')
            {
                inQuotes = inQuotes is false;
                continue;
            }

            if (char.IsWhiteSpace(character) && inQuotes is false)
            {
                if (current.Length > 0)
                {
                    tokens.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(character);
            }
        }
        
        if (current.Length > 0)
            tokens.Add(current.ToString());

        return tokens.ToArray();
    }
    
    public void Dispose()
    {
        Plugin.CommandManager.RemoveHandler(CommandNameShort);
        Plugin.CommandManager.RemoveHandler(CommandNameFull);
        Plugin.CommandManager.RemoveHandler(CommandNameOld);
        
        GC.SuppressFinalize(this);
    }
}