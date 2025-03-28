using System;
using System.Runtime.InteropServices;
using System.Text;
using FFXIVClientStructs.FFXIV.Client.System.String;
using FFXIVClientStructs.FFXIV.Client.UI;

namespace AetherRemoteClient.Services;

/// <summary>
///     Provides access to sending messages to the XIV chat
/// </summary>
public class ChatService
{
    // Const
    const AllowedEntities AllowedCharacters = AllowedEntities.UppercaseLetters | 
                                              AllowedEntities.LowercaseLetters | 
                                              AllowedEntities.Numbers |
                                              AllowedEntities.SpecialCharacters |
                                              AllowedEntities.CharacterList | 
                                              AllowedEntities.OtherCharacters |
                                              AllowedEntities.Payloads | 
                                              AllowedEntities.Unknown8 | 
                                              AllowedEntities.Unknown9;
    
    private readonly ProcessChatBoxDelegate? _processChatBox;

    /// <summary>
    ///     <inheritdoc cref="ChatService" />
    /// </summary>
    public ChatService()
    {
        if (Plugin.SigScanner.TryScanText(Signatures.SendChat, out var processChatBoxPtr))
            _processChatBox = Marshal.GetDelegateForFunctionPointer<ProcessChatBoxDelegate>(processChatBoxPtr);
    }

    /// <summary>
    ///     Sends a message to the chat, sanitizing the message and performing light validation
    /// </summary>
    /// <param name="message">Message to send, must be non-empty and less than 500 characters</param>
    public void SendMessage(string message)
    {
        try
        {
            var bytes = Encoding.UTF8.GetBytes(SanitiseText(message));
            switch (bytes.Length)
            {
                case 0:
                    Plugin.Log.Warning("[ChatProvider] Attempted to send an empty message");
                    return;
                case > 500:
                    Plugin.Log.Warning("[ChatProvider] Attempted to send a message longer than 500 bytes");
                    return;
                default:
                    SendMessageUnsafe(bytes);
                    break;
            }
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"[ChatProvider] An exception occured while sending a message: {exception}");
        }
    }

    private unsafe void SendMessageUnsafe(byte[] message)
    {
        if (_processChatBox is null)
            throw new InvalidOperationException("Could not find signature for chat sending");

        try
        {
            var uiModule = (IntPtr)UIModule.Instance();

            using var payload = new ChatPayload(message);
            var messageMemory = Marshal.AllocHGlobal(400);

            Marshal.StructureToPtr(payload, messageMemory, false);
            _processChatBox(uiModule, messageMemory, IntPtr.Zero, 0);
            Marshal.FreeHGlobal(messageMemory);
        }
        catch (Exception exception)
        {
            Plugin.Log.Error($"[ChatProvider] An exception occured while sending an unsafe message: {exception}");
        }
    }

    private static unsafe string SanitiseText(string text)
    {
        var utf8String = Utf8String.FromString(text);
        utf8String->SanitizeString(0x27F, (Utf8String*)nint.Zero);

        var sanitised = utf8String->ToString();
        utf8String->Dtor(true);

        return sanitised;
    }

    private static class Signatures
    {
        internal const string SendChat = "48 89 5C 24 ?? 48 89 74 24 ?? 57 48 83 EC ?? 48 8B F2 48 8B F9 45 84 C9";
    }

    private delegate void ProcessChatBoxDelegate(IntPtr uiModule, IntPtr message, IntPtr unused, byte a4);

    [StructLayout(LayoutKind.Explicit)]
    private readonly struct ChatPayload : IDisposable
    {
        [FieldOffset(0)] private readonly IntPtr textPtr;

        [FieldOffset(8)] private readonly ulong unk1;

        [FieldOffset(16)] private readonly ulong textLen;

        [FieldOffset(24)] private readonly ulong unk2;

        internal ChatPayload(byte[] stringBytes)
        {
            textPtr = Marshal.AllocHGlobal(stringBytes.Length + 30);
            Marshal.Copy(stringBytes, 0, textPtr, stringBytes.Length);
            Marshal.WriteByte(textPtr + stringBytes.Length, 0);

            textLen = (ulong)(stringBytes.Length + 1);

            unk1 = 64;
            unk2 = 0;
        }

        public void Dispose()
        {
            Marshal.FreeHGlobal(textPtr);
        }
    }
}