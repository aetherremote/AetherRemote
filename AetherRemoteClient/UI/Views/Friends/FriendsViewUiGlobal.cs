using System.Numerics;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;

namespace AetherRemoteClient.UI.Views.Friends;

public partial class FriendsViewUi
{
    private void DrawGlobalPermissions(float width)
    {
        var half = width * 0.5f;
        var quarter = width * 0.25f;
        var third = width * 0.75f;
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalSave", AetherRemoteStyle.PanelBackground, true, () =>
        {
            ImGui.Button("Save Changes", new Vector2(width - AetherRemoteImGui.WindowPadding.X * 2,  AetherRemoteDimensions.SendCommandButtonHeight));
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
            
            ImGui.TextUnformatted("Linkshell Permissions");
            ImGui.Checkbox("Ls1", ref controller.Global.LinkshellValues[0]); ImGui.SameLine(quarter);
            ImGui.Checkbox("Ls2", ref controller.Global.LinkshellValues[1]); ImGui.SameLine(half);
            ImGui.Checkbox("Ls3", ref controller.Global.LinkshellValues[2]); ImGui.SameLine(third);
            ImGui.Checkbox("Ls4", ref controller.Global.LinkshellValues[3]); 
            ImGui.Checkbox("Ls5", ref controller.Global.LinkshellValues[4]); ImGui.SameLine(quarter);
            ImGui.Checkbox("Ls6", ref controller.Global.LinkshellValues[5]); ImGui.SameLine(half);
            ImGui.Checkbox("Ls7", ref controller.Global.LinkshellValues[6]); ImGui.SameLine(third);
            ImGui.Checkbox("Ls8", ref controller.Global.LinkshellValues[7]);
            
            ImGui.Spacing();
            
            ImGui.TextUnformatted("Cross-world Linkshell Permissions");
            ImGui.Checkbox("Cwls1", ref controller.Global.CrossWorldLinkshellValues[0]); ImGui.SameLine(quarter);
            ImGui.Checkbox("Cwls2", ref controller.Global.CrossWorldLinkshellValues[1]); ImGui.SameLine(half);
            ImGui.Checkbox("Cwls3", ref controller.Global.CrossWorldLinkshellValues[2]); ImGui.SameLine(third);
            ImGui.Checkbox("Cwls4", ref controller.Global.CrossWorldLinkshellValues[3]);
            ImGui.Checkbox("Cwls5", ref controller.Global.CrossWorldLinkshellValues[4]); ImGui.SameLine(quarter);
            ImGui.Checkbox("Cwls6", ref controller.Global.CrossWorldLinkshellValues[5]); ImGui.SameLine(half);
            ImGui.Checkbox("Cwls7", ref controller.Global.CrossWorldLinkshellValues[6]); ImGui.SameLine(third);
            ImGui.Checkbox("Cwls8", ref controller.Global.CrossWorldLinkshellValues[7]);
        });

        SharedUserInterfaces.ContentBox("PermissionsGlobalElevated", AetherRemoteStyle.ElevatedBackground, false, () =>
        {
            ImGui.TextUnformatted("Elevated Permissions");
            ImGui.Checkbox("Permanent Transformations", ref controller.Global.PermanentTransformationValue); ImGui.SameLine(half);
            ImGui.Checkbox("Possession", ref controller.Global.PossessionValue);
        });
    }
}