using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Friends;

/// <summary>
///     Handles UI elements for the Friends tab
/// </summary>
public class FriendsViewUi(
    FriendsListComponentUi friendsList, 
    FriendsViewUiController controller,
    SelectionManager selectionManager) : IDrawable
{
    private const string UnsavedChangesText = "You have unsaved changes";
    private static readonly Vector2 Half = new(0.5f);
    private static readonly Vector2 IconSize = new(40);
    private static readonly Vector2 SmallIconSize = new(24, 0);

    public void Draw()
    {
        ImGui.BeginChild("PermissionContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        var windowWidth = ImGui.GetWindowWidth();
        var windowPadding = ImGui.GetStyle().WindowPadding;

        if (selectionManager.Selected.Count is not 1)
        {
            SharedUserInterfaces.ContentBox("A2", AetherRemoteStyle.PanelBackground, true, () =>
            {
                SharedUserInterfaces.TextCentered(selectionManager.Selected.Count is 0
                    ? "Select a friend to edit"
                    : "You must only edit a single person's permissions");
            });

            ImGui.EndChild();
            ImGui.SameLine();
            friendsList.Draw(true, true);
            return;
        }

        SharedUserInterfaces.ContentBox("FriendsHeader", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, AetherRemoteStyle.Rounding);
            ImGui.PushStyleVar(ImGuiStyleVar.ButtonTextAlign, Half);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, IconSize) && (ImGui.IsKeyDown(ImGuiKey.RightAlt) || ImGui.IsKeyDown(ImGuiKey.LeftAlt)))
                controller.Delete();
            SharedUserInterfaces.Tooltip("Hold Alt to remove friend");
            
            ImGui.SameLine(windowWidth - windowPadding.X * 2 - IconSize.X);
            
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Save, IconSize))
                controller.Save();
            SharedUserInterfaces.Tooltip("Save permissions");

            ImGui.SameLine();
            ImGui.PopStyleVar(2);

            SharedUserInterfaces.BigTextCentered($"{controller.FriendCode}");
            ImGui.InputTextWithHint("###FriendNoteTextInput", "Note", ref controller.Note, 128);
        });

        SharedUserInterfaces.ContentBox("FriendsSpeakOptions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            var buttonRowY = ImGui.GetCursorPosY();
            ImGui.TextUnformatted("Channels");

            if (ImGui.BeginTable("SpeakPermissionsTable", 3))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Say", ref controller.EditingUserPermissions.Say);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Roleplay", ref controller.EditingUserPermissions.Roleplay);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Echo", ref controller.EditingUserPermissions.Echo);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Yell", ref controller.EditingUserPermissions.Yell);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Shout", ref controller.EditingUserPermissions.Shout);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Tell", ref controller.EditingUserPermissions.Tell);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Party", ref controller.EditingUserPermissions.Party);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Alliance", ref controller.EditingUserPermissions.Alliance);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Free Company", ref controller.EditingUserPermissions.FreeCompany);
                ImGui.TableNextColumn();
                ImGui.Checkbox("PVP Team", ref controller.EditingUserPermissions.PvPTeam);
                ImGui.EndTable();
            }

            ImGui.TextUnformatted("Linkshells");
            ImGui.Checkbox("1##Ls", ref controller.EditingUserPermissions.Ls1);
            ImGui.SameLine();
            ImGui.Checkbox("2##Ls", ref controller.EditingUserPermissions.Ls2);
            ImGui.SameLine();
            ImGui.Checkbox("3##Ls", ref controller.EditingUserPermissions.Ls3);
            ImGui.SameLine();
            ImGui.Checkbox("4##Ls", ref controller.EditingUserPermissions.Ls4);
            ImGui.SameLine();
            ImGui.Checkbox("5##Ls", ref controller.EditingUserPermissions.Ls5);
            ImGui.SameLine();
            ImGui.Checkbox("6##Ls", ref controller.EditingUserPermissions.Ls6);
            ImGui.SameLine();
            ImGui.Checkbox("7##Ls", ref controller.EditingUserPermissions.Ls7);
            ImGui.SameLine();
            ImGui.Checkbox("8##Ls", ref controller.EditingUserPermissions.Ls8);

            ImGui.TextUnformatted("Cross-world Linkshells");
            ImGui.Checkbox("1##Cwl1", ref controller.EditingUserPermissions.Cwl1);
            ImGui.SameLine();
            ImGui.Checkbox("2##Cwl2", ref controller.EditingUserPermissions.Cwl2);
            ImGui.SameLine();
            ImGui.Checkbox("3##Cwl3", ref controller.EditingUserPermissions.Cwl3);
            ImGui.SameLine();
            ImGui.Checkbox("4##Cwl4", ref controller.EditingUserPermissions.Cwl4);
            ImGui.SameLine();
            ImGui.Checkbox("5##Cwl5", ref controller.EditingUserPermissions.Cwl5);
            ImGui.SameLine();
            ImGui.Checkbox("6##Cwl6", ref controller.EditingUserPermissions.Cwl6);
            ImGui.SameLine();
            ImGui.Checkbox("7##Cwl7", ref controller.EditingUserPermissions.Cwl7);
            ImGui.SameLine();
            ImGui.Checkbox("8##Cwl8", ref controller.EditingUserPermissions.Cwl8);

            var current = ImGui.GetCursorPos();
            ImGui.SetCursorPos(new Vector2(windowWidth - (windowPadding.X + SmallIconSize.X) * 2, buttonRowY));

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Check, SmallIconSize))
                controller.SetAllSpeakPermissions(true);
            SharedUserInterfaces.Tooltip("Allow all");
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Ban, SmallIconSize))
                controller.SetAllSpeakPermissions(false);
            SharedUserInterfaces.Tooltip("Allow none");
            ImGui.SetCursorPos(current);
        });

        SharedUserInterfaces.ContentBox("FriendsGeneralOptions", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.TextUnformatted("General Permissions");
            if (ImGui.BeginTable("GeneralPermissionsTable", 2))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Emotes", ref controller.EditingUserPermissions.Emote);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Hypnosis", ref controller.EditingUserPermissions.Hypnosis);
                ImGui.EndTable();
            }
        });
        
        // TODO: Change includeEndPadding once elevated permissions are back
        SharedUserInterfaces.ContentBox("FriendsCharacterAttributesOptions", AetherRemoteStyle.PanelBackground, false, () =>
        {
            ImGui.TextUnformatted("Character Attributes");
            if (ImGui.BeginTable("CharacterAttributesPermissionsTable", 2))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Customization", ref controller.EditingUserPermissions.Customization);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Equipment", ref controller.EditingUserPermissions.Equipment);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Mods", ref controller.EditingUserPermissions.Mods);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Body Swap", ref controller.EditingUserPermissions.BodySwap);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Twinning", ref controller.EditingUserPermissions.Twinning);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Moodles", ref controller.EditingUserPermissions.Moodles);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Customize+", ref controller.EditingUserPermissions.CustomizePlus);
                
                ImGui.EndTable();
            }
        });
        
        /*
        SharedUserInterfaces.ContentBox("FriendsElevatedOptions", AetherRemoteStyle.ElevatedBackground, false, () =>
        {
            ImGui.TextUnformatted("Elevated Permissions");
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.ExclamationCircle);
            SharedUserInterfaces.Tooltip("Elevated permissions allow for more intrusive functionality and should be granted carefully");
            
            if (ImGui.BeginTable("CharacterAttributesPermissionsTable", 2))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Permanent Transformations", ref controller.EditingUserPermissions.PermanentTransformation);
                SharedUserInterfaces.Tooltip(
                    [
                        "Allows this friend to lock your appearance, preventing changes until you enter the PIN they created", 
                        "You can unlock it from the Status tab or by using the safeword command or safe mode"
                    ]);
                ImGui.EndTable();
            }
        });
        */

        // Pending Changes
        if (controller.PendingChanges())
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
        ImGui.SameLine();
        friendsList.Draw(true, true);
    }
}