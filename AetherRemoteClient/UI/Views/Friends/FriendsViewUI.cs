using System;
using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Friends;

/// <summary>
///     Handles UI elements for the Friends tab
/// </summary>
public class FriendsViewUi(FriendsListService friendsListService, NetworkService networkService)
    : IDrawable, IDisposable
{
    private readonly FriendsViewUiController _controller = new(friendsListService, networkService);

    private const string UnsavedChangesText = "You have unsaved changes";
    private static readonly Vector2 Half = new(0.5f);
    private static readonly Vector2 IconSize = new(40);
    private static readonly Vector2 SmallIconSize = new(24, 0);

    public bool Draw()
    {
        ImGui.BeginChild("PermissionContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        var windowWidth = ImGui.GetWindowWidth();
        var windowPadding = ImGui.GetStyle().WindowPadding;

        if (friendsListService.Selected.Count is not 1)
        {
            SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
            {
                SharedUserInterfaces.TextCentered(friendsListService.Selected.Count is 0
                    ? "Select a friend to edit"
                    : "You must only edit a single person's permissions");
            }, true, false);

            ImGui.EndChild();
            return true;
        }

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, AetherRemoteStyle.Rounding);
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, Half);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, IconSize) && (ImGui.IsKeyDown(ImGuiKey.RightAlt) || ImGui.IsKeyDown(ImGuiKey.LeftAlt)))
                _controller.Delete();
            SharedUserInterfaces.Tooltip("Hold Alt to remove friend");
            
            ImGui.SameLine(windowWidth - windowPadding.X * 2 - IconSize.X);
            
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Save, IconSize))
                _controller.Save();
            SharedUserInterfaces.Tooltip("Save permissions");

            ImGui.SameLine();
            ImGui.PopStyleVar(2);

            SharedUserInterfaces.BigTextCentered($"{_controller.FriendCode}");
            ImGui.InputTextWithHint("###FriendNoteTextInput", "Note", ref _controller.Note, 128);
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            var buttonRowY = ImGui.GetCursorPosY();
            ImGui.TextUnformatted("Speak");
            ImGui.Checkbox("Allow##Speak", ref _controller.EditingUserPermissions.Speak);

            ImGui.TextUnformatted("Channels");

            var shouldDisableSpeakTable = _controller.EditingUserPermissions.Speak is false;
            if (shouldDisableSpeakTable)
                ImGui.BeginDisabled();

            if (ImGui.BeginTable("SpeakPermissionsTable", 3))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Say", ref _controller.EditingUserPermissions.Say);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Chat Emote", ref _controller.EditingUserPermissions.ChatEmote);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Echo", ref _controller.EditingUserPermissions.Echo);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Yell", ref _controller.EditingUserPermissions.Yell);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Shout", ref _controller.EditingUserPermissions.Shout);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Tell", ref _controller.EditingUserPermissions.Tell);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Party", ref _controller.EditingUserPermissions.Party);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Alliance", ref _controller.EditingUserPermissions.Alliance);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Free Company", ref _controller.EditingUserPermissions.FreeCompany);
                ImGui.TableNextColumn();
                ImGui.Checkbox("PVP Team", ref _controller.EditingUserPermissions.PvPTeam);
                ImGui.EndTable();
            }

            ImGui.TextUnformatted("Linkshells");
            ImGui.Checkbox("1##Ls", ref _controller.EditingUserPermissions.Ls1);
            ImGui.SameLine();
            ImGui.Checkbox("2##Ls", ref _controller.EditingUserPermissions.Ls2);
            ImGui.SameLine();
            ImGui.Checkbox("3##Ls", ref _controller.EditingUserPermissions.Ls3);
            ImGui.SameLine();
            ImGui.Checkbox("4##Ls", ref _controller.EditingUserPermissions.Ls4);
            ImGui.SameLine();
            ImGui.Checkbox("5##Ls", ref _controller.EditingUserPermissions.Ls5);
            ImGui.SameLine();
            ImGui.Checkbox("6##Ls", ref _controller.EditingUserPermissions.Ls6);
            ImGui.SameLine();
            ImGui.Checkbox("7##Ls", ref _controller.EditingUserPermissions.Ls7);
            ImGui.SameLine();
            ImGui.Checkbox("8##Ls", ref _controller.EditingUserPermissions.Ls8);

            ImGui.TextUnformatted("Cross-world Linkshells");
            ImGui.Checkbox("1##Cwl1", ref _controller.EditingUserPermissions.Cwl1);
            ImGui.SameLine();
            ImGui.Checkbox("2##Cwl2", ref _controller.EditingUserPermissions.Cwl2);
            ImGui.SameLine();
            ImGui.Checkbox("3##Cwl3", ref _controller.EditingUserPermissions.Cwl3);
            ImGui.SameLine();
            ImGui.Checkbox("4##Cwl4", ref _controller.EditingUserPermissions.Cwl4);
            ImGui.SameLine();
            ImGui.Checkbox("5##Cwl5", ref _controller.EditingUserPermissions.Cwl5);
            ImGui.SameLine();
            ImGui.Checkbox("6##Cwl6", ref _controller.EditingUserPermissions.Cwl6);
            ImGui.SameLine();
            ImGui.Checkbox("7##Cwl7", ref _controller.EditingUserPermissions.Cwl7);
            ImGui.SameLine();
            ImGui.Checkbox("8##Cwl8", ref _controller.EditingUserPermissions.Cwl8);

            if (shouldDisableSpeakTable)
                ImGui.EndDisabled();

            var current = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(windowWidth - (windowPadding.X + SmallIconSize.X) * 2, buttonRowY));

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Check, SmallIconSize))
                _controller.SetAllSpeakPermissions(true);
            SharedUserInterfaces.Tooltip("Allow all");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Ban, SmallIconSize))
                _controller.SetAllSpeakPermissions(false);
            SharedUserInterfaces.Tooltip("Allow none");
            ImGui.SetCursorPos(current);
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            ImGui.TextUnformatted("Emotes");
            ImGui.Checkbox("Allow###Emotes", ref _controller.EditingUserPermissions.Emote);
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            ImGui.TextUnformatted("Transformation");
            if (ImGui.BeginTable("TransformationPermissionsTable", 2))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Customization", ref _controller.EditingUserPermissions.Customization);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Equipment", ref _controller.EditingUserPermissions.Equipment);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Body Swap", ref _controller.EditingUserPermissions.BodySwap);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Twinning", ref _controller.EditingUserPermissions.Twinning);
                ImGui.EndTable();
            }

            ImGui.TextUnformatted("Mod Swapping");
            ImGui.Checkbox("Allow###ModSwapping", ref _controller.EditingUserPermissions.Mods);
        }, true, false);

        // Pending Changes
        if (_controller.PendingChanges())
        {
            var drawList = ImGui.GetWindowDrawList();
            drawList.ChannelsSplit(2);

            drawList.ChannelsSetCurrent(1);
            var textSize = ImGui.CalcTextSize(UnsavedChangesText);
            var pos = ImGui.GetWindowPos();
            var size = ImGui.GetWindowSize();
            var final = new Vector2(pos.X + (size.X - textSize.X) * 0.5f, pos.Y + windowPadding.Y * 2);
            drawList.AddText(final, ImGui.ColorConvertFloat4ToU32(Vector4.One), UnsavedChangesText);

            drawList.ChannelsSetCurrent(0);
            var min = final - windowPadding;
            var max = final + textSize + windowPadding;
            drawList.AddRectFilled(min, max, ImGui.ColorConvertFloat4ToU32(new Vector4(0, 0, 0, 1)),
                AetherRemoteStyle.Rounding);
            drawList.AddRect(min, max, ImGui.ColorConvertFloat4ToU32(ImGuiColors.DalamudOrange),
                AetherRemoteStyle.Rounding);
            drawList.AddRect(pos, pos + size, ImGui.ColorConvertFloat4ToU32(ImGuiColors.DalamudOrange),
                AetherRemoteStyle.Rounding);

            drawList.ChannelsMerge();
        }

        ImGui.EndChild();
        return true;
    }

    public void Dispose()
    {
        _controller.Dispose();
        GC.SuppressFinalize(this);
    }
}