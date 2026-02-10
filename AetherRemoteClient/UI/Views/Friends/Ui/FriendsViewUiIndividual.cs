using System.Collections.Generic;
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
    private static readonly Dictionary<uint, string> LinkshellLabels = new()
    {
        { 0, "Ls1" },
        { 1, "Ls2" },
        { 2, "Ls3" },
        { 3, "Ls4" },
        { 4, "Ls5" },
        { 5, "Ls6" },
        { 6, "Ls7" },
        { 7, "Ls8" }
    };
    
    private static readonly Dictionary<uint, string> CrossWorldLabels = new()
    {
        { 0, "Cwl1" },
        { 1, "Cwl2" },
        { 2, "Cwl3" },
        { 3, "Cwl4" },
        { 4, "Cwl5" },
        { 5, "Cwl6" },
        { 6, "Cwl7" },
        { 7, "Cwl8" }
    };
    
    private void DrawIndividualPermissions(float width)
    {
        var denyPosition = width * 0.5f;
        var inheritPosition = width * 0.65f;
        var allowPosition = width * 0.8f;

        var count = selection.Selected.Count;
        
        // No one selected
        if (count is 0)
        {
            SharedUserInterfaces.ContentBox("PermissionsIndividualSelectOne", AetherRemoteColors.PanelColor, true, () =>
            {
                SharedUserInterfaces.TextCentered("You must select a friend");
            });
            return;
        }

        // More than one person selected
        if (count > 1)
        {
            SharedUserInterfaces.ContentBox("PermissionsIndividualOnlyOne", AetherRemoteColors.PanelColor, true, () =>
            {
                SharedUserInterfaces.TextCentered("You may only edit one friend at a time");
            });
        }

        SharedUserInterfaces.ContentBox("PermissionsIndividualNote", AetherRemoteColors.PanelColor, true, () =>
        {
            SharedUserInterfaces.MediumText(selection.Selected.FirstOrDefault() is not { } friend ? "Note" : $"Note for {friend.FriendCode}");
            ImGui.SetNextItemWidth(width - AetherRemoteImGui.WindowPadding.X * 2);
            ImGui.InputText("##Note", ref controller.Individual.Note, 32);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsIndividualPrimary", AetherRemoteColors.PanelColor, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Primary Permissions"); ImGui.SameLine(denyPosition);
            ImGui.TextUnformatted("Deny"); ImGui.SameLine(inheritPosition);
            ImGui.TextUnformatted("Default"); ImGui.SameLine(allowPosition);
            ImGui.TextUnformatted("Allow");
            ImGui.Separator();
            
            DrawIndividualPermissionRow("Body Swap", denyPosition, inheritPosition, allowPosition, ref controller.Individual.BodySwapValue);
            DrawIndividualPermissionRow("Customize+", denyPosition, inheritPosition, allowPosition, ref controller.Individual.CustomizePlusValue);
            DrawIndividualPermissionRow("Emote", denyPosition, inheritPosition, allowPosition, ref controller.Individual.EmoteValue);
            DrawIndividualPermissionRow("Glamourer Customizations", denyPosition, inheritPosition, allowPosition, ref controller.Individual.GlamourerCustomizationsValue);
            DrawIndividualPermissionRow("Glamourer Equipment", denyPosition, inheritPosition, allowPosition, ref controller.Individual.GlamourerEquipmentValue);
            DrawIndividualPermissionRow("Honorific", denyPosition, inheritPosition, allowPosition, ref controller.Individual.HonorificValue);
            DrawIndividualPermissionRow("Hypnosis", denyPosition, inheritPosition, allowPosition, ref controller.Individual.HypnosisValue);
            DrawIndividualPermissionRow("Moodles", denyPosition, inheritPosition, allowPosition, ref controller.Individual.MoodlesValue);
            DrawIndividualPermissionRow("Penumbra Mods", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PenumbraModsValue);
            DrawIndividualPermissionRow("Twinning", denyPosition, inheritPosition, allowPosition, ref controller.Individual.TwinningValue);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsIndividualSpeak", AetherRemoteColors.PanelColor, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Speak Permissions"); ImGui.SameLine(denyPosition);
            ImGui.TextUnformatted("Deny"); ImGui.SameLine(inheritPosition);
            ImGui.TextUnformatted("Inherit"); ImGui.SameLine(allowPosition);
            ImGui.TextUnformatted("Allow");
            ImGui.Separator();
            
            DrawIndividualPermissionRow("Alliance", denyPosition, inheritPosition, allowPosition, ref controller.Individual.AllianceValue);
            DrawIndividualPermissionRow("Echo", denyPosition, inheritPosition, allowPosition, ref controller.Individual.EchoValue);
            DrawIndividualPermissionRow("Free Company", denyPosition, inheritPosition, allowPosition, ref controller.Individual.FreeCompanyValue);
            DrawIndividualPermissionRow("Party", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PartyValue);
            DrawIndividualPermissionRow("PvP Team", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PvPTeamValue);
            DrawIndividualPermissionRow("Roleplay", denyPosition, inheritPosition, allowPosition, ref controller.Individual.RoleplayValue);
            DrawIndividualPermissionRow("Say", denyPosition, inheritPosition, allowPosition, ref controller.Individual.SayValue);
            DrawIndividualPermissionRow("Shout", denyPosition, inheritPosition, allowPosition, ref controller.Individual.ShoutValue);
            DrawIndividualPermissionRow("Tell", denyPosition, inheritPosition, allowPosition, ref controller.Individual.TellValue);
            DrawIndividualPermissionRow("Yell", denyPosition, inheritPosition, allowPosition, ref controller.Individual.YellValue);
            
            ImGui.Spacing();
            
            ImGui.TextUnformatted("Linkshell Permissions");
            ImGui.Separator();
            for (uint index = 0; index < 8; index++)
                DrawIndividualLinkshellPermissionRow(index, true, denyPosition, inheritPosition, allowPosition, ref controller.Individual.LinkshellValues[index]);
            
            ImGui.Spacing();
            
            ImGui.TextUnformatted("Cross-world Linkshell Permissions");
            ImGui.Separator();
            for (uint index = 0; index < 8; index++)
                DrawIndividualLinkshellPermissionRow(index, false, denyPosition, inheritPosition, allowPosition, ref controller.Individual.CrossWorldLinkshellValues[index]);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsIndividualElevated", AetherRemoteColors.PrimaryColor, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Elevated Permissions"); ImGui.SameLine(denyPosition);
            ImGui.TextUnformatted("Deny"); ImGui.SameLine(inheritPosition);
            ImGui.TextUnformatted("Inherit"); ImGui.SameLine(allowPosition);
            ImGui.TextUnformatted("Allow");
            ImGui.Separator();
            
            // DrawIndividualPermissionButton("Permanent Transformations", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PermanentTransformationValue);
            DrawIndividualPermissionRow("Possession", denyPosition, inheritPosition, allowPosition, ref controller.Individual.PossessionValue);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsIndividualSave", AetherRemoteColors.PanelColor, false, () =>
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
    
    private static void DrawIndividualPermissionRow(string label, float denyPosition, float inheritPosition, float allowPosition, ref PermissionValue value)
    {
        ImGui.TextUnformatted(label); ImGui.SameLine(denyPosition);
        ImGui.PushID(label);

        // Deny
        var denySelected = value == PermissionValue.Deny;
        if (denySelected)
            ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.DalamudRed);

        if (ImGui.RadioButton("##Deny", denySelected))
            value = PermissionValue.Deny;

        if (denySelected)
            ImGui.PopStyleColor();
        
        ImGui.SameLine(inheritPosition);

        // Inherit
        var inheritSelected = value == PermissionValue.Inherit;
        if (ImGui.RadioButton("##Inherit", inheritSelected))
            value = PermissionValue.Inherit;

        ImGui.SameLine(allowPosition);

        // Allow
        var allowSelected = value == PermissionValue.Allow;
        if (allowSelected)
            ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.HealerGreen);

        if (ImGui.RadioButton("##Allow", allowSelected))
            value = PermissionValue.Allow;

        if (allowSelected)
            ImGui.PopStyleColor();

        ImGui.PopID();
    }
    
    private static void DrawIndividualLinkshellPermissionRow(uint index, bool linkshell, float denyPosition, float inheritPosition, float allowPosition, ref PermissionValue value)
    {
        // Label
        ImGui.TextUnformatted(linkshell ? GetLinkshellName(index) : GetCrossWorldLinkshellName(index));
        ImGui.SameLine(denyPosition);
        
        // Id
        ImGui.PushID(linkshell ? LinkshellLabels[index] : CrossWorldLabels[index]);

        // Deny
        var denySelected = value == PermissionValue.Deny;
        if (denySelected)
            ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.DalamudRed);

        if (ImGui.RadioButton("##Deny", denySelected))
            value = PermissionValue.Deny;

        if (denySelected)
            ImGui.PopStyleColor();

        ImGui.SameLine(inheritPosition);

        // Inherit
        var inheritSelected = value == PermissionValue.Inherit;
        if (ImGui.RadioButton("##Inherit", inheritSelected))
            value = PermissionValue.Inherit;

        ImGui.SameLine(allowPosition);

        // Allow
        var allowSelected = value == PermissionValue.Allow;
        if (allowSelected)
            ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.HealerGreen);

        if (ImGui.RadioButton("##Allow", allowSelected))
            value = PermissionValue.Allow;

        if (allowSelected)
            ImGui.PopStyleColor();

        ImGui.PopID();
    }
}