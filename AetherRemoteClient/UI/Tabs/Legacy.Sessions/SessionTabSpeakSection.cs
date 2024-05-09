using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonChatMode;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;
using System;
using System.Text;
using System.Threading.Tasks;

namespace AetherRemoteClient.UI.Experimental.Tabs.Sessions.Speak;

public class SessionTabSpeakSection(NetworkProvider networkProvider)
{
    // Const
    private static readonly int LinkshellSelectorWidth = 42;

    // Injected
    private readonly NetworkProvider networkProvider = networkProvider;

    // Local
    private ChatMode chatMode = ChatMode.Say;
    private int shellNumber = 1;
    private string tellTarget = "";
    private string message = "";
    private Session? currentSession = null;

    private readonly StringBuilder sb = new();

    public void SetSession(Session newSession)
    {
        currentSession = newSession;
    }

    public void DrawSpeakSection()
    {
        var shouldProcessSpeakCommand = false;

        SharedUserInterfaces.MediumText(chatMode.ToCondensedString(), ImGuiColors.ParsedOrange);

        if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(LinkshellSelectorWidth);

            if (ImGui.BeginCombo("###LinkshellSelector", shellNumber.ToString()))
            {
                for (var i = 1; i < 9; i++)
                {
                    if (ImGui.Selectable(i.ToString(), shellNumber == i))
                    {
                        shellNumber = i;
                    }
                }

                ImGui.EndCombo();
            }
        }
        else if (chatMode == ChatMode.Tell)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(200);
            ImGui.InputTextWithHint("###TellTarget", "Target", ref tellTarget, Constants.PlayerNameCharLimit);
        }

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Comment))
        {
            ImGui.OpenPopup("ChatModeSelector");
        }

        if (ImGui.BeginPopup("ChatModeSelector"))
        {
            foreach (ChatMode mode in Enum.GetValues(typeof(ChatMode)))
            {
                if (ImGui.Selectable(mode.ToCondensedString(), mode == chatMode))
                {
                    chatMode = mode;
                }
            }

            ImGui.EndPopup();
        }

        ImGui.SameLine();

        ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - 100);
        if (ImGui.InputTextWithHint("###MessageInputBox", "Message", ref message, 400, ImGuiInputTextFlags.EnterReturnsTrue))
            shouldProcessSpeakCommand = true;

        ImGui.SameLine();

        ImGui.SetNextItemWidth(50);
        if (ImGui.Button("Send"))
            shouldProcessSpeakCommand = true;

        if (shouldProcessSpeakCommand)
            _ = ProcessSpeakCommand();
    }

    private async Task ProcessSpeakCommand()
    {
        if (currentSession == null)
            return;

        if (message.Length <= 0)
            return;

        string? extra = null;
        if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
        {
            extra = shellNumber.ToString();
        }
        else if (chatMode == ChatMode.Tell)
        {
            if (tellTarget.Length > 0)
                extra = tellTarget;
        }

        var secret = "";// secretProvider.Secret;
        var targets = currentSession.TargetFriends;
        var result = await networkProvider.Speak(secret, targets, message, chatMode, extra);
        if (result.Success)
        {
            sb.Clear();
            sb.Append("You made ");
            sb.Append(currentSession.TargetFriendsAsList());
            if (chatMode == ChatMode.Tell)
            {
                sb.Append("send a tell to ");
                sb.Append(extra);
                sb.Append(" saying: \"");
                sb.Append(message);
                sb.Append("\".");
            }
            else
            {
                sb.Append("say: \"");
                sb.Append(message);
                sb.Append("\" in ");
                sb.Append(chatMode.ToCondensedString());
                if (chatMode == ChatMode.Linkshell || chatMode == ChatMode.CrossworldLinkshell)
                {
                    sb.Append(extra);
                }
                sb.Append('.');
            }

            AetherRemoteLogging.Log("Me", sb.ToString(), DateTime.Now, LogType.Sent);
        }
    }
}
