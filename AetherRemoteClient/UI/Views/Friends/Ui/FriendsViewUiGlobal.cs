using System.Numerics;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Friends.Ui;

public partial class FriendsViewUi
{
    private void DrawGlobalPermissions(float width)
    {
        var half = width * 0.5f;
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalSave", AetherRemoteStyle.PanelBackground, true, () =>
        {
            if (ImGui.Button("Save Changes", new Vector2(width - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight)))
                _ = controller.SaveGlobalPermissions().ConfigureAwait(false);
            
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Save your global permissions");
        });
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalPrimary", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.TextUnformatted("Primary Permissions");
            ImGui.Checkbox("Body Swap", ref controller.Global.BodySwapValue); ImGui.SameLine(half);
            ImGui.Checkbox("Customize+", ref controller.Global.CustomizePlusValue);
            ImGui.Checkbox("Emote", ref controller.Global.EmoteValue); ImGui.SameLine(half);
            ImGui.Checkbox("Glamourer Customizations", ref controller.Global.GlamourerCustomizationsValue);
            ImGui.Checkbox("Glamourer Equipment", ref controller.Global.GlamourerEquipmentValue); ImGui.SameLine(half);
            ImGui.Checkbox("Honorific", ref controller.Global.HonorificValue);
            ImGui.Checkbox("Hypnosis", ref controller.Global.HypnosisValue); ImGui.SameLine(half);
            ImGui.Checkbox("Moodles", ref controller.Global.MoodlesValue);
            ImGui.Checkbox("Penumbra Mods", ref controller.Global.PenumbraModsValue); ImGui.SameLine(half);
            ImGui.Checkbox("Twinning", ref controller.Global.TwinningValue);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalSpeak", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.TextUnformatted("Speak Permissions");
            ImGui.Checkbox("Alliance", ref controller.Global.AllianceValue); ImGui.SameLine(half);
            ImGui.Checkbox("Echo", ref controller.Global.EchoValue);
            ImGui.Checkbox("Free Company", ref controller.Global.FreeCompanyValue); ImGui.SameLine(half);
            ImGui.Checkbox("Party", ref controller.Global.PartyValue);
            ImGui.Checkbox("PvP Team", ref controller.Global.PvPTeamValue); ImGui.SameLine(half);
            ImGui.Checkbox("Roleplay", ref controller.Global.RoleplayValue);
            ImGui.Checkbox("Say", ref controller.Global.SayValue); ImGui.SameLine(half);
            ImGui.Checkbox("Shout", ref controller.Global.ShoutValue);
            ImGui.Checkbox("Tell", ref controller.Global.TellValue); ImGui.SameLine(half);
            ImGui.Checkbox("Yell", ref controller.Global.YellValue);
            
            ImGui.Spacing();
            
            ImGui.BeginGroup();
            ImGui.TextUnformatted("Linkshell Permissions");
            for (uint i = 0; i < 8; i++)
                ImGui.Checkbox($"[{i + 1}]: {GetLinkshellName(i)}##Ls", ref controller.Global.LinkshellValues[i]);
            ImGui.EndGroup();
            
            ImGui.SameLine(half);
            
            ImGui.BeginGroup();
            ImGui.TextUnformatted("Cross-World Permissions");
            for (uint i = 0; i < 8; i++)
                ImGui.Checkbox($"[{i + 1}]: {GetCrossWorldLinkshellName(i)}##Cwls", ref controller.Global.CrossWorldLinkshellValues[i]);
            ImGui.EndGroup();
        });

        SharedUserInterfaces.ContentBox("PermissionsGlobalElevated", AetherRemoteStyle.ElevatedBackground, false, () =>
        {
            ImGui.TextUnformatted("Elevated Permissions");
            ImGui.Checkbox("Permanent Transformations", ref controller.Global.PermanentTransformationValue); ImGui.SameLine(half);
            ImGui.Checkbox("Possession", ref controller.Global.PossessionValue);
        });
    }
}