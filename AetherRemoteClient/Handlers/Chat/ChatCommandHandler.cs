using System;
using System.Collections.Generic;
using AetherRemoteClient.Dependencies.CustomizePlus.Services;
using AetherRemoteClient.Dependencies.Glamourer.Services;
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
                           /ar {Customize} | targets | profile name | optional: merge
                                - Must use friend codes when targeting
                                - Profile name is case sensitive
                                - Example: /ar customize | FriendCodeOne, FriendCodeTwo | My Profile Name
                                - Example: /ar customize | FriendCodeOne | Mimic Profile | merge
                           /ar {Emote} | targets | emote | optional: display a log message
                                - Must use friend codes when targeting
                                - Emote is case sensitive, type it like you would in chat
                                - Example: /ar emote | FriendCode | dance
                                - Example: /ar emote | FriendCodeOne, FriendCodeThree | dance | true
                           /ar {Speak} | targets | channel | message
                                - Must use friend codes when targeting
                                - Channel is case sensitive, type it like you would in chat
                                - Example: /ar speak | FriendCode | tell My Name@My World | Hello how are you?
                                - Example: /ar speak | FriendCode | cwl1 | Roulette?
                           /ar {Transform} | targets | design name | optional: apply type
                                - Must use friend codes when targeting
                                - Design is case sensitive
                                - Apply type options are customize, equipment, both (blank defaults to both)
                                - Example: /ar transform | FriendCode | My Design Name
                                - Example: /Ar transform | FriendCode | Farming Glam | equipment
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
                
                case Customize:
                    _ = HandleCustomize(args).ConfigureAwait(false);
                    break;
                
                case Emote:
                    _ = HandleEmote(args).ConfigureAwait(false);
                    break;
                
                case Speak:
                    _ = HandleSpeak(args).ConfigureAwait(false);
                    break;
                
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
    
    /// <summary>
    ///     Sends a message in chat that looks like "[AetherRemote] Message"
    /// </summary>
    private static void SendChatMessage(string message)
    {
        var payloads = new List<Payload>
        {
            new UIForegroundPayload(AetherRemoteColors.TextColorPurple),
            new TextPayload("[AetherRemote] "),
            UIForegroundPayload.UIForegroundOff,
            new TextPayload(message)
        };
            
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