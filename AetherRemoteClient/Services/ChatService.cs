using System;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to sending messages to the XIV chat
/// </summary>
public static class ChatService
{
    // Const
    private const AllowedEntities Allowed = AllowedEntities.UppercaseLetters |
                                            AllowedEntities.LowercaseLetters |
                                            AllowedEntities.Numbers |
                                            AllowedEntities.SpecialCharacters |
                                            AllowedEntities.CharacterList |
                                            AllowedEntities.OtherCharacters |
                                            AllowedEntities.Payloads |
                                            AllowedEntities.Unknown8 |
                                            AllowedEntities.Unknown9;

    /// <summary>
    ///     Sends a message to the chat, sanitizing the message and performing light validation
    /// </summary>
    /// <param name="message">Message to send, must be non-empty and less than 500 characters</param>
    public static void SendMessage(string message)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(message);
            switch (bytes.Length)
            {
                case 0:
                    Plugin.Log.Warning("[ChatProvider] Attempted to send an empty message");
                    return;
                
                case > 500:
                    Plugin.Log.Warning("[ChatProvider] Attempted to send a message longer than 500 bytes");
                    return;
                
                default:
                    SendMessageUnsafe(message);
                    break;
            }
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"[ChatProvider] An exception occured while sending a message: {exception}");
        }
    }

    private static unsafe void SendMessageUnsafe(string message)
    {
        var utf8String = Utf8String.FromString(message);
        utf8String->SanitizeString(Allowed, null);
        UIModule.Instance()->ProcessChatBoxEntry(utf8String);
        utf8String->Dtor(true);
    }
}