using System.Linq;
using System.Numerics;
using AetherRemoteClient.Domain.Enums;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Friends.Ui;

public partial class FriendsViewUi
{
    private void DrawIndividualPermissions(float width)
    {
        var denyPosition = width * 0.5f;
        var inheritPosition = width * 0.65f;
        var allowPosition = width * 0.8f;

        switch (selection.Selected.Count)
        {
            case 0:
                SharedUserInterfaces.ContentBox("PermissionsIndividualSelectOne", AetherRemoteStyle.PanelBackground, true, () =>
                {
                    SharedUserInterfaces.TextCentered("You must select a friend");
                });
                return;
            
            case >1:
                SharedUserInterfaces.ContentBox("PermissionsIndividualOnlyOne", AetherRemoteStyle.PanelBackground, true, () =>
                {
                    SharedUserInterfaces.TextCentered("You may only edit one friend at a time");
                });
                return;
        }
        
        SharedUserInterfaces.ContentBox("PermissionsIndividualNote", AetherRemoteStyle.PanelBackground, true, () =>
        {
            SharedUserInterfaces.MediumText(selection.Selected.FirstOrDefault() is not { } friend ? "Note" : $"Note for {friend.FriendCode}");
            ImGui.SetNextItemWidth(width - AetherRemoteImGui.WindowPadding.X * 2);
            ImGui.InputText("##Note", ref controller.Individual.Note, 32);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsIndividualPrimary", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Primary Permissions"); ImGui.SameLine(denyPosition);
            ImGui.TextUnformatted("Deny"); ImGui.SameLine(inheritPosition);
            ImGui.TextUnformatted("Default"); ImGui.SameLine(allowPosition);
            ImGui.TextUnformatted("Allow");
            ImGui.Separator();
            
            DrawIndividualPermissionButton("Body Swap", denyPosition, inheritPosition, allowPosition, ref controller.Individual.BodySwapValue);
            DrawIndividualPermissionButton("Customize+", denyPosition, inheritPosition, allowPosition, ref controller.Individual.CustomizePlusValue);
            DrawIndividualPermissionButton("Emote", denyPosition, inheritPosition, allowPosition, ref controller.Individual.EmoteValue);
            DrawIndividualPermissionButton("Glamourer Customizations", denyPosition, inheritPosition, allowPosition, ref controller.Individual.GlamourerCustomizationsValue);
            DrawIndividualPermissionButton("Glamourer Equipment", denyPosition, inheritPosition, allowPosition, ref controller.Individual.GlamourerEquipmentValue);
            DrawIndividualPermissionButton("Honorific", denyPosition, inheritPosition, allowPosition, ref controller.Individual.HonorificValue);
            DrawIndividualPermissionButton("Hypnosis", denyPosition, inheritPosition, allowPosition, ref controller.Individual.HypnosisValue);
            DrawIndividualPermissionButton("Moodles", denyPosition, inheritPosition, allowPosition, ref controller.Individual.MoodlesValue);
            DrawIndividualPermissionButton("Penumbra Mods", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PenumbraModsValue);
            DrawIndividualPermissionButton("Twinning", denyPosition, inheritPosition, allowPosition, ref controller.Individual.TwinningValue);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsIndividualSpeak", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Speak Permissions"); ImGui.SameLine(denyPosition);
            ImGui.TextUnformatted("Deny"); ImGui.SameLine(inheritPosition);
            ImGui.TextUnformatted("Inherit"); ImGui.SameLine(allowPosition);
            ImGui.TextUnformatted("Allow");
            ImGui.Separator();
            
            DrawIndividualPermissionButton("Alliance", denyPosition, inheritPosition, allowPosition, ref controller.Individual.AllianceValue);
            DrawIndividualPermissionButton("Echo", denyPosition, inheritPosition, allowPosition, ref controller.Individual.EchoValue);
            DrawIndividualPermissionButton("Free Company", denyPosition, inheritPosition, allowPosition, ref controller.Individual.FreeCompanyValue);
            DrawIndividualPermissionButton("Party", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PartyValue);
            DrawIndividualPermissionButton("PvP Team", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PvPTeamValue);
            DrawIndividualPermissionButton("Roleplay", denyPosition, inheritPosition, allowPosition, ref controller.Individual.RoleplayValue);
            DrawIndividualPermissionButton("Say", denyPosition, inheritPosition, allowPosition, ref controller.Individual.SayValue);
            DrawIndividualPermissionButton("Shout", denyPosition, inheritPosition, allowPosition, ref controller.Individual.ShoutValue);
            DrawIndividualPermissionButton("Tell", denyPosition, inheritPosition, allowPosition, ref controller.Individual.TellValue);
            DrawIndividualPermissionButton("Yell", denyPosition, inheritPosition, allowPosition, ref controller.Individual.YellValue);
            
            ImGui.Spacing();
            
            ImGui.TextUnformatted("Linkshell Permissions");
            ImGui.Separator();
            for (uint index = 0; index < 8; index++)
                DrawIndividualLinkshellButton(index, true, denyPosition, inheritPosition, allowPosition, ref controller.Individual.LinkshellValues[index]);
            
            ImGui.Spacing();
            
            ImGui.TextUnformatted("Cross-world Linkshell Permissions");
            ImGui.Separator();
            for (uint index = 0; index < 8; index++)
                DrawIndividualLinkshellButton(index, false, denyPosition, inheritPosition, allowPosition, ref controller.Individual.CrossWorldLinkshellValues[index]);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsIndividualElevated", AetherRemoteStyle.ElevatedBackground, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Elevated Permissions"); ImGui.SameLine(denyPosition);
            ImGui.TextUnformatted("Deny"); ImGui.SameLine(inheritPosition);
            ImGui.TextUnformatted("Inherit"); ImGui.SameLine(allowPosition);
            ImGui.TextUnformatted("Allow");
            ImGui.Separator();
            
            // DrawIndividualPermissionButton("Permanent Transformations", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PermanentTransformationValue);
            DrawIndividualPermissionButton("Possession", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PossessionValue);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsIndividualSave", AetherRemoteStyle.PanelBackground, false, () =>
        {
            if (ImGui.Button("Save Changes", new Vector2(width - AetherRemoteImGui.WindowPadding.X * 3 - AetherRemoteDimensions.SendCommandButtonHeight, AetherRemoteDimensions.SendCommandButtonHeight)))
                _ = controller.SaveIndividualPermissions().ConfigureAwait(false);
            
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Save changes for this friend");
            
            ImGui.SameLine();

            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Trash, new Vector2(AetherRemoteDimensions.SendCommandButtonHeight)))
                _ = controller.DeleteIndividualPermissions().ConfigureAwait(false);
            
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Remove this friend");
        });
    }
    
    /// <summary>
    ///     Draws the radio buttons to make up the three options for an individual permission
    /// </summary>
    private static void DrawIndividualPermissionButton(string permission, float denyPosition, float inheritPosition, float allowPosition, ref PermissionValue value)
    {
        ImGui.TextUnformatted(permission); ImGui.SameLine(denyPosition);

        switch (value)
        {
            case PermissionValue.Deny:
                ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.DalamudRed);
                ImGui.RadioButton($"##Deny{permission}", true);
                ImGui.PopStyleColor();
                ImGui.SameLine(inheritPosition);

                if (ImGui.RadioButton($"##Inherit{permission}", false))
                    value = PermissionValue.Inherit;
                ImGui.SameLine(allowPosition);
        
                if (ImGui.RadioButton($"##Allow{permission}", false))
                    value = PermissionValue.Allow;
                break;
            
            case PermissionValue.Inherit:
                if (ImGui.RadioButton($"##Deny{permission}", false))
                    value = PermissionValue.Deny;
                ImGui.SameLine(inheritPosition);
                
                ImGui.RadioButton($"##Inherit{permission}", true);
                ImGui.SameLine(allowPosition);
                
                if (ImGui.RadioButton($"##Allow{permission}", false))
                    value = PermissionValue.Allow;
                break;
            
            case PermissionValue.Allow:
                if (ImGui.RadioButton($"##Deny{permission}", false))
                    value = PermissionValue.Deny;
                ImGui.SameLine(inheritPosition);
        
                if (ImGui.RadioButton($"##Inherit{permission}", false))
                    value = PermissionValue.Inherit;
                ImGui.SameLine(allowPosition);
        
                ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.HealerGreen);
                ImGui.RadioButton($"##Allow{permission}", true);
                ImGui.PopStyleColor();
                break;
            
            default:
                ImGui.TextUnformatted("Unknown PermissionValue");
                break;
        }
    }
    
    /// <summary>
    ///     Draws the radio buttons to make up the three options for an individual permission
    /// </summary>
    private static void DrawIndividualLinkshellButton(uint index, bool linkshell, float denyPosition, float inheritPosition, float allowPosition, ref PermissionValue value)
    {
        var name = linkshell ? GetLinkshellName(index) : GetCrossWorldLinkshellName(index);
        ImGui.TextUnformatted($"[{index + 1}]: {name}"); ImGui.SameLine(denyPosition);

        var prefix = linkshell ? "Ls" : "Cwls";
        switch (value)
        {
            case PermissionValue.Deny:
                ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.DalamudRed);
                ImGui.RadioButton($"##Deny{prefix}{index}", true);
                ImGui.PopStyleColor();
                ImGui.SameLine(inheritPosition);

                if (ImGui.RadioButton($"##Inherit{prefix}{index}", false))
                    value = PermissionValue.Inherit;
                ImGui.SameLine(allowPosition);
        
                if (ImGui.RadioButton($"##Allow{prefix}{index}", false))
                    value = PermissionValue.Allow;
                break;
            
            case PermissionValue.Inherit:
                if (ImGui.RadioButton($"##Deny{prefix}{index}", false))
                    value = PermissionValue.Deny;
                ImGui.SameLine(inheritPosition);
                
                ImGui.RadioButton($"##Inherit{prefix}{index}", true);
                ImGui.SameLine(allowPosition);
                
                if (ImGui.RadioButton($"##Allow{prefix}{index}", false))
                    value = PermissionValue.Allow;
                break;
            
            case PermissionValue.Allow:
                if (ImGui.RadioButton($"##Deny{prefix}{index}", false))
                    value = PermissionValue.Deny;
                ImGui.SameLine(inheritPosition);
        
                if (ImGui.RadioButton($"##Inherit{prefix}{index}", false))
                    value = PermissionValue.Inherit;
                ImGui.SameLine(allowPosition);
        
                ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.HealerGreen);
                ImGui.RadioButton($"##Allow{prefix}{index}", true);
                ImGui.PopStyleColor();
                break;
            
            default:
                ImGui.TextUnformatted("Unknown PermissionValue");
                break;
        }
    }
}