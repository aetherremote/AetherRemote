using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Events;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using AetherRemoteCommon.Domain.CommonFriend;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Collections.Generic;
using System.Numerics;

namespace AetherRemoteClient.UI.Tabs.Friends;

public class FriendsTab : ITab
{
    // Constants
    private static readonly Vector2 RoundButtonSize = new(40, 40);
    private static readonly Vector2 SmallButtonSize = new(24, 0);
    private static readonly Vector4 IconOnlineColor = ImGuiColors.ParsedGreen;
    private static readonly Vector4 IconOfflineColor = ImGuiColors.DPSRed;

    // Dependencies
    private readonly Configuration configuration;
    private readonly NetworkProvider networkProvider;
    private readonly IPluginLog logger;

    // Events
    public static event EventHandler<FriendDeletedEventArgs>? OnFriendDeleted;

    public FriendsTab(Configuration configuration, NetworkProvider networkProvider, IPluginLog logger)
    {
        this.configuration = configuration;
        this.networkProvider = networkProvider;
        this.logger = logger;

        friendSearchFilter = new(networkProvider, FilterFriend);
    }

    /// <summary>
    /// The string being referenced by the friend code input text
    /// </summary>
    private string friendCodeAddFriendInputText = "";

    /// <summary>
    /// The string being refered by the search input text
    /// </summary>
    private string friendCodeSearchInputText = "";

    /// <summary>
    /// The friend whose settings are currently being editted
    /// </summary>
    private Friend? friendBeingEditted = null;

    /// <summary>
    /// Threaded filter for searching your friend list
    /// </summary>
    private readonly FriendListFilter friendSearchFilter;

    /// <summary>
    /// Should the currently editted friend be deleted
    /// </summary>
    private bool shouldDeleteFriend = false;

    // Friend being edit's note
    private string friendNote = string.Empty;

    // Permissions - Speak
    private bool allowSpeak = false;
    private bool allowSay = false;
    private bool allowYell = false;
    private bool allowShout = false;
    private bool allowTell = false;
    private bool allowParty = false;
    private bool allowAlliance = false;
    private bool allowFreeCompany = false;
    private bool allowLinkshell = false;
    private bool allowCrossworldLinkshell = false;
    private bool allowPvPTeam = false;

    // Permissions - Emote
    private bool allowEmote = false;

    // Permissions - Glamourer
    private bool allowChangeAppearance = false;
    private bool allowChangeEquipment = false;

    public void Draw()
    {
        // Set deletion status each frame
        shouldDeleteFriend = false;

        // Grab a reference to the style
        var style = ImGui.GetStyle();

        // The height of the footer containing the friend code input text and the add friend button
        var footerHeight = (ImGui.CalcTextSize("Add Friend").Y + (style.FramePadding.Y * 2) + style.ItemSpacing.Y) * 2;

        if (ImGui.BeginTabItem("Friends"))
        {
            // Draw the settings area beside the search bar using the remaining space
            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##SearchFriendListInputText", "Search", ref friendCodeSearchInputText, Constants.FriendNicknameCharLimit))
                friendSearchFilter.UpdateSearchTerm(friendCodeSearchInputText);

            // Save the cursor at the bottom of the search input text before calling ImGui.SameLine for use later
            var bottomOfSearchInputText = ImGui.GetCursorPosY();

            ImGui.SameLine();

            // Draw the settings area beside the search bar using the remaining space
            if (ImGui.BeginChild("FriendSettingsArea", Vector2.Zero, true))
            {
                DrawFriendSetting();
                ImGui.EndChild();
            }

            // Set the cursor back and begin drawing add friend input text & button
            ImGui.SetCursorPosY(bottomOfSearchInputText);

            ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(0, 2));

            // By setting the Y value as negative, the window will be that many pixels up from the bottom
            if (ImGui.BeginChild("FriendListArea", new Vector2(MainWindow.FriendListSize.X, -1 * footerHeight), true))
            {
                DrawFriendList();
                ImGui.EndChild();
            }

            ImGui.PopStyleVar();

            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            if (ImGui.InputTextWithHint("##FriendCodeInputText", "Friend Code", ref friendCodeAddFriendInputText, Constants.FriendCodeCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
                ProcessAddFriend();

            if (ImGui.Button("Add Friend", MainWindow.FriendListSize))
                ProcessAddFriend();

            ImGui.EndTabItem();
        }

        ProcessDeleteFriend();
    }

    private void DrawFriendList()
    {
        var onlineFriends = new List<Friend>();
        var offlineFriends = new List<Friend>();
        foreach (var friend in friendSearchFilter.List)
        {
            if (friend.Online) onlineFriends.Add(friend);
            else offlineFriends.Add(friend);
        }

        if (ImGui.TreeNodeEx($"Online ({onlineFriends.Count})", ImGuiTreeNodeFlags.DefaultOpen))
        {
            foreach (var friend in onlineFriends)
                DrawSelectableFriend(friend);

            ImGui.TreePop();
        }

        if (ImGui.TreeNodeEx($"Offline ({offlineFriends.Count})"))
        {
            foreach (var friend in offlineFriends)
                DrawSelectableFriend(friend);

            ImGui.TreePop();
        }
    }

    private void DrawFriendSetting()
    {
        if (friendBeingEditted == null)
        {
            SharedUserInterfaces.PushBigFont();
            var windowCenter = new Vector2(ImGui.GetWindowSize().X / 2, ImGui.GetWindowSize().Y / 2);
            var textSize = ImGui.CalcTextSize("Select Friend");
            var cursorPos = new Vector2(windowCenter.X - (textSize.X / 2), windowCenter.Y - textSize.Y);
            ImGui.SetCursorPos(cursorPos);
            ImGui.Text("Select Friend");
            SharedUserInterfaces.PopBigFont();

            SharedUserInterfaces.TextCentered("Start by selecting a friend from the left");
        }
        else
        {
            SharedUserInterfaces.BigTextCentered(friendBeingEditted.FriendCode, ImGuiColors.ParsedOrange);

            ImGui.SameLine();
            var deleteButtonPosition = ImGui.GetWindowSize() - ImGui.GetStyle().WindowPadding - RoundButtonSize;
            ImGui.SetCursorPosX(deleteButtonPosition.X);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, RoundButtonSize) && friendBeingEditted != null)
                shouldDeleteFriend = true;

            ImGui.PopStyleVar();
            SharedUserInterfaces.Tooltip("Delete Friend");

            ImGui.Separator();

            SharedUserInterfaces.TextCentered("Details");

            ImGui.SetNextItemWidth(MainWindow.FriendListSize.X);
            ImGui.InputTextWithHint("Note##EditingFriendCode", "Note", ref friendNote, Constants.FriendCodeCharLimit);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            SharedUserInterfaces.Tooltip("A note or 'nickname' to more easily identify a friend");

            ImGui.Separator();
            SharedUserInterfaces.TextCentered("General Permissions");

            ImGui.Checkbox("Allow force speak", ref allowSpeak);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            SharedUserInterfaces.Tooltip("Allow friend to force you to say things in chat");

            ImGui.SameLine();
            ImGui.SetCursorPosX(ImGui.GetWindowSize().X - (ImGui.GetStyle().WindowPadding.X * 2) - (SmallButtonSize.X * 2));

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Check, SmallButtonSize))
                SetAllSpeakPermissions(true);

            SharedUserInterfaces.Tooltip("Allow All");

            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Ban, SmallButtonSize))
                SetAllSpeakPermissions(false);

            SharedUserInterfaces.Tooltip("Disallow All");

            ImGui.Indent();
            if (allowSpeak == false) ImGui.BeginDisabled();
            if (ImGui.BeginTable("SpeakPermissionsTable", 2))
            {
                ImGui.TableNextColumn(); ImGui.Checkbox("Say", ref allowSay);
                ImGui.TableNextColumn(); ImGui.Checkbox("Yell", ref allowYell);
                ImGui.TableNextColumn(); ImGui.Checkbox("Shout", ref allowShout);
                ImGui.TableNextColumn(); ImGui.Checkbox("Tell", ref allowTell);
                ImGui.TableNextColumn(); ImGui.Checkbox("Party", ref allowParty);
                ImGui.TableNextColumn(); ImGui.Checkbox("Alliance", ref allowAlliance);
                ImGui.TableNextColumn(); ImGui.Checkbox("Free Company", ref allowFreeCompany);
                ImGui.TableNextColumn(); ImGui.Checkbox("PVP Team", ref allowPvPTeam);
                ImGui.TableNextColumn(); ImGui.Checkbox("Linkshells", ref allowLinkshell);
                ImGui.TableNextColumn(); ImGui.Checkbox("Crosworld Linkshells", ref allowCrossworldLinkshell);
                ImGui.EndTable();
            }
            ImGui.Unindent();
            if (allowSpeak == false) ImGui.EndDisabled();

            ImGui.Checkbox("Allow force emote", ref allowEmote);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            SharedUserInterfaces.Tooltip("Allow friend to force you to perform emotes");

            ImGui.Separator();
            SharedUserInterfaces.TextCentered("Glamourer Permissions");

            ImGui.Checkbox("Allow change appearance", ref allowChangeAppearance);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            SharedUserInterfaces.Tooltip("Allow friend to change your character's appearance using glamourer");

            ImGui.Checkbox("Allow change equipment", ref allowChangeEquipment);
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            SharedUserInterfaces.Tooltip("Allow friend to change your character's equipment using glamourer");

            var saveButtonPosition = ImGui.GetWindowSize() - ImGui.GetStyle().WindowPadding - RoundButtonSize;
            ImGui.SetCursorPos(saveButtonPosition);
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 100f);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Save, RoundButtonSize) && friendBeingEditted != null)
                ProcessSaveFriend();

            SharedUserInterfaces.Tooltip("Save Friend");

            if (PendingChanges())
            {
                ImGui.SetCursorPosY(ImGui.GetWindowHeight() - ImGui.GetFontSize() - ImGui.GetStyle().WindowPadding.Y);
                SharedUserInterfaces.TextCentered("You have pending changes!", ImGuiColors.DalamudOrange);
            }

            ImGui.PopStyleVar();
        }
    }

    private void DrawSelectableFriend(Friend friend)
    {
        var onlineStatus = friend.Online;
        ImGui.SetCursorPosX(8 + ImGui.GetFontSize() + (ImGui.GetStyle().FramePadding.X * 2));

        // Draw Selectable Text
        if (onlineStatus == false) ImGui.PushStyleColor(ImGuiCol.Text, ImGuiColors.DalamudGrey);
        if (ImGui.Selectable($"{friend.NoteOrFriendCode}", friendBeingEditted == friend, ImGuiSelectableFlags.SpanAllColumns))
            EditFriend(friend);
        if (onlineStatus == false) ImGui.PopStyleColor();

        // Draw Icon
        ImGui.SameLine(8);
        ImGui.PushStyleColor(ImGuiCol.Text, onlineStatus ? IconOnlineColor : IconOfflineColor);
        SharedUserInterfaces.Icon(FontAwesomeIcon.User);
        ImGui.PopStyleColor();
    }

    private async void ProcessAddFriend()
    {
        if (friendCodeAddFriendInputText.Length <= 0)
            return;

        var findFriendResult = networkProvider.FriendList?.FindFriend(friendCodeAddFriendInputText);
        if (findFriendResult != null) // Grumble.. I hate using != but grammatically nothing else makes readable sense..
        {
            friendCodeAddFriendInputText = "";
            logger.Warning($"[AetherRemote] Error adding friend: Already friends");
            return;
        }

        var networkAddResult = await networkProvider.CreateOrUpdateFriend(configuration.Secret, friendCodeAddFriendInputText);
        if (networkAddResult.Success == false)
        {
            friendCodeAddFriendInputText = "";
            logger.Warning ($"[AetherRemote] Error adding friend: {networkAddResult.Message}");
            return;
        }

        var localAddResult = networkProvider.FriendList?.CreateAndAddFriend(friendCodeAddFriendInputText, networkAddResult.Online) ?? false;
        if (localAddResult == false)
        {
            friendCodeAddFriendInputText = "";
            logger.Error("[AetherRemote] Error adding friend. Desync has occurred.");
            return;
        }
        
        // Clear textbox
        friendCodeAddFriendInputText = "";
    }

    private async void ProcessDeleteFriend()
    {
        if (shouldDeleteFriend == false) return;
        if (friendBeingEditted == null) return;
        if (networkProvider.FriendList == null) return;

        var friendCode = friendBeingEditted.FriendCode;
        var serverDeleteResult = await networkProvider.DeleteFriend(configuration.Secret, friendCode);
        if (serverDeleteResult.Success == false)
            return;

        // Invoke Event
        OnFriendDeleted?.Invoke(this, new(friendCode));
        
        networkProvider.FriendList.RemoveFriend(friendCode);
        friendBeingEditted = null;
    }

    private void EditFriend(Friend friend)
    {
        friendBeingEditted = friend;

        friendNote = friend.Note ?? string.Empty;
        allowSpeak = friend.Permissions.Speak;
        allowEmote = friend.Permissions.Emote;
        allowChangeAppearance = friend.Permissions.ChangeAppearance;
        allowChangeEquipment = friend.Permissions.ChangeEquipment;
        allowSay = friend.Permissions.Say;
        allowYell = friend.Permissions.Yell;
        allowShout = friend.Permissions.Shout;
        allowTell = friend.Permissions.Tell;
        allowParty = friend.Permissions.Party;
        allowAlliance = friend.Permissions.Alliance;
        allowFreeCompany = friend.Permissions.FreeCompany;
        allowLinkshell = friend.Permissions.Linkshell;
        allowCrossworldLinkshell = friend.Permissions.CrossworldLinkshell;
        allowPvPTeam = friend.Permissions.PvPTeam;
    }

    private async void ProcessSaveFriend()
    {
        if (friendBeingEditted == null)
            return;

        var converted = friendBeingEditted.Copy();
        converted.Note = friendNote == string.Empty ? null : friendNote;
        converted.Permissions.SetAll(allowEmote, allowSpeak, allowChangeAppearance, allowChangeEquipment, allowSay,
            allowYell, allowShout, allowTell, allowParty, allowAlliance,
            allowFreeCompany, allowLinkshell, allowCrossworldLinkshell, allowPvPTeam);

        var result = await networkProvider.CreateOrUpdateFriend(configuration.Secret, converted);
        if (result.Success == false)
            return;
        
        friendBeingEditted.Note = friendNote == string.Empty ? null : friendNote;
        friendBeingEditted.Permissions.SetAll(allowEmote, allowSpeak, allowChangeAppearance, allowChangeEquipment, 
            allowSay, allowYell, allowShout, allowTell, allowParty, allowAlliance,
            allowFreeCompany, allowLinkshell, allowCrossworldLinkshell, allowPvPTeam);
    }

    private void SetAllSpeakPermissions(bool enabled)
    {
        allowSpeak = enabled; allowSay = enabled;
        allowYell = enabled; allowShout = enabled;
        allowTell = enabled;
        allowParty = enabled;
        allowAlliance = enabled;
        allowFreeCompany = enabled;
        allowPvPTeam = enabled;
        allowLinkshell = enabled;
        allowCrossworldLinkshell = enabled;
    }

    /// <summary>
    /// Checks to see if the friend being editted has pending changes
    /// </summary>
    private bool PendingChanges()
    {
        if (friendBeingEditted == null)
            return false;

        // Note
        if (friendBeingEditted.Note == null)
        {
            if (friendNote != string.Empty)
                return true;
        }
        else
        {
            if (friendBeingEditted.Note != friendNote)
                return true;
        }

        if (friendBeingEditted.Permissions.Speak != allowSpeak) return true;
        if (friendBeingEditted.Permissions.Say != allowSay) return true;
        if (friendBeingEditted.Permissions.Yell != allowYell) return true;
        if (friendBeingEditted.Permissions.Shout != allowShout) return true;
        if (friendBeingEditted.Permissions.Tell != allowTell) return true;
        if (friendBeingEditted.Permissions.Party != allowParty) return true;
        if (friendBeingEditted.Permissions.Alliance != allowAlliance) return true;
        if (friendBeingEditted.Permissions.FreeCompany != allowFreeCompany) return true;
        if (friendBeingEditted.Permissions.PvPTeam != allowPvPTeam) return true;
        if (friendBeingEditted.Permissions.Linkshell != allowLinkshell) return true;
        if (friendBeingEditted.Permissions.CrossworldLinkshell != allowCrossworldLinkshell) return true;
        if (friendBeingEditted.Permissions.Emote != allowEmote) return true;
        if (friendBeingEditted.Permissions.ChangeAppearance != allowChangeAppearance) return true;
        if (friendBeingEditted.Permissions.ChangeEquipment != allowChangeEquipment) return true;

        return false;
    }

    private static bool FilterFriend(Friend friend, string searchTerm)
    {
        var foundFriendCode = friend.FriendCode.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
        if (foundFriendCode)
            return true;

        if (friend.Note == null)
            return false;

        return friend.Note.Contains(searchTerm, StringComparison.OrdinalIgnoreCase);
    }

    public void Dispose()
    {
        friendSearchFilter.Dispose();
        GC.SuppressFinalize(this);
    }
}
