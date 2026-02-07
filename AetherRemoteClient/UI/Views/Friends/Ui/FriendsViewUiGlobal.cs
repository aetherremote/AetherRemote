using System.Numerics;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Friends.Ui;

public partial class FriendsViewUi
{
    private void DrawGlobalPermissions(float width)
    {
        var offPosition = width * 0.65f;
        var onPosition = width * 0.8f;
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalPrimary", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Primary Permissions"); ImGui.SameLine(offPosition);
            ImGui.TextUnformatted("Off"); ImGui.SameLine(onPosition);
            ImGui.TextUnformatted("On");
            ImGui.Separator();

            DrawGlobalPermissionButton("Body Swap", offPosition, onPosition, ref controller.Global.BodySwapValue);
            DrawGlobalPermissionButton("Customize+", offPosition, onPosition, ref controller.Global.CustomizePlusValue);
            DrawGlobalPermissionButton("Emote", offPosition, onPosition, ref controller.Global.EmoteValue);
            DrawGlobalPermissionButton("Glamourer Customizations", offPosition, onPosition, ref controller.Global.GlamourerCustomizationsValue);
            DrawGlobalPermissionButton("Glamourer Equipment", offPosition, onPosition, ref controller.Global.GlamourerEquipmentValue);
            DrawGlobalPermissionButton("Honorific", offPosition, onPosition, ref controller.Global.HonorificValue);
            DrawGlobalPermissionButton("Hypnosis", offPosition, onPosition, ref controller.Global.HypnosisValue);
            DrawGlobalPermissionButton("Moodles", offPosition, onPosition, ref controller.Global.MoodlesValue);
            DrawGlobalPermissionButton("Penumbra Mods", offPosition, onPosition, ref controller.Global.PenumbraModsValue);
            DrawGlobalPermissionButton("Twinning", offPosition, onPosition, ref controller.Global.TwinningValue);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalSpeak", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Speak Permissions"); ImGui.SameLine(offPosition);
            ImGui.TextUnformatted("Off"); ImGui.SameLine(onPosition);
            ImGui.TextUnformatted("On");
            ImGui.Separator();

            DrawGlobalPermissionButton("Alliance", offPosition, onPosition, ref controller.Global.AllianceValue);
            DrawGlobalPermissionButton("Echo", offPosition, onPosition, ref controller.Global.EchoValue);
            DrawGlobalPermissionButton("Free Company", offPosition, onPosition, ref controller.Global.FreeCompanyValue);
            DrawGlobalPermissionButton("Party", offPosition, onPosition, ref controller.Global.PartyValue);
            DrawGlobalPermissionButton("PvP Team", offPosition, onPosition, ref controller.Global.PvPTeamValue);
            DrawGlobalPermissionButton("Roleplay", offPosition, onPosition, ref controller.Global.RoleplayValue);
            DrawGlobalPermissionButton("Say", offPosition, onPosition, ref controller.Global.SayValue);
            DrawGlobalPermissionButton("Shout", offPosition, onPosition, ref controller.Global.ShoutValue);
            DrawGlobalPermissionButton("Tell", offPosition, onPosition, ref controller.Global.TellValue);
            DrawGlobalPermissionButton("Yell", offPosition, onPosition, ref controller.Global.YellValue);
            
            ImGui.Spacing();
            
            ImGui.TextUnformatted("Linkshell Permissions");
            ImGui.Separator();
            for (uint i = 0; i < 8; i++)
                DrawGlobalPermissionButton($"[{i + 1}]: {GetLinkshellName(i)}", offPosition, onPosition, ref controller.Global.LinkshellValues[i]);
            
            ImGui.Spacing();
            
            ImGui.TextUnformatted("Cross-world Linkshell Permissions");
            ImGui.Separator();
            for (uint i = 0; i < 8; i++)
                DrawGlobalPermissionButton($"[{i + 1}]: {GetCrossWorldLinkshellName(i)}", offPosition, onPosition, ref controller.Global.CrossWorldLinkshellValues[i]);
        });

        SharedUserInterfaces.ContentBox("PermissionsGlobalElevated", AetherRemoteStyle.ElevatedBackground, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Elevated Permissions"); ImGui.SameLine(offPosition);
            ImGui.TextUnformatted("Off"); ImGui.SameLine(onPosition);
            ImGui.TextUnformatted("On");
            ImGui.Separator();
            
            // DrawGlobalPermissionButton("Permanent Transformations", offPosition, onPosition, ref controller.Global.PermanentTransformationValue);
            DrawGlobalPermissionButton("Possession", offPosition, onPosition, ref controller.Global.PossessionValue);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalSave", AetherRemoteStyle.PanelBackground, false, () =>
        {
            if (ImGui.Button("Save Changes", new Vector2(width - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight)))
                _ = controller.SaveGlobalPermissions().ConfigureAwait(false);
            
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Save your global permissions");
        });
    }
    
    /// <summary>
    ///     Draws the radio buttons to make up the three options for an individual permission
    /// </summary>
    private static void DrawGlobalPermissionButton(string permission, float offPosition, float onPosition, ref bool value)
    {
        ImGui.TextUnformatted(permission); ImGui.SameLine(offPosition);

        if (value)
        {
            if (ImGui.RadioButton($"##Off{permission}", false))
                value = false;
            
            ImGui.SameLine(onPosition);
            
            ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.HealerGreen);
            ImGui.RadioButton($"##On{permission}", true);
            ImGui.PopStyleColor();
        }
        else
        {
            ImGui.RadioButton($"##Off{permission}", true);
            
            ImGui.SameLine(onPosition);

            if (ImGui.RadioButton($"##On{permission}", false))
                value = true;
        }
    }
}