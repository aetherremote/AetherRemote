using System.Numerics;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Friends;

public partial class FriendsView
{
    private void DrawGlobalPermissions(float width)
    {
        var offPosition = width * 0.65f;
        var onPosition = width * 0.8f;
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalPrimary", AetherRemoteColors.PanelColor, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Primary Permissions"); ImGui.SameLine(offPosition);
            ImGui.TextUnformatted("Off"); ImGui.SameLine(onPosition);
            ImGui.TextUnformatted("On");
            ImGui.Separator();

            DrawGlobalPermissionButton("Body Swap", offPosition, onPosition, ref _global.BodySwapValue);
            DrawGlobalPermissionButton("Customize+", offPosition, onPosition, ref _global.CustomizePlusValue);
            DrawGlobalPermissionButton("Emote", offPosition, onPosition, ref _global.EmoteValue);
            DrawGlobalPermissionButton("Glamourer", offPosition, onPosition, ref _global.GlamourerValue);
            DrawGlobalPermissionButton("Honorific", offPosition, onPosition, ref _global.HonorificValue);
            DrawGlobalPermissionButton("Hypnosis", offPosition, onPosition, ref _global.HypnosisValue);
            DrawGlobalPermissionButton("Moodles", offPosition, onPosition, ref _global.MoodlesValue);
            DrawGlobalPermissionButton("Penumbra Mods", offPosition, onPosition, ref _global.PenumbraModsValue);
            DrawGlobalPermissionButton("Twinning", offPosition, onPosition, ref _global.TwinningValue);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalSpeak", AetherRemoteColors.PanelColor, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Speak Permissions"); ImGui.SameLine(offPosition);
            ImGui.TextUnformatted("Off"); ImGui.SameLine(onPosition);
            ImGui.TextUnformatted("On");
            ImGui.Separator();

            DrawGlobalPermissionButton("Alliance", offPosition, onPosition, ref _global.AllianceValue);
            DrawGlobalPermissionButton("Echo", offPosition, onPosition, ref _global.EchoValue);
            DrawGlobalPermissionButton("Free Company", offPosition, onPosition, ref _global.FreeCompanyValue);
            DrawGlobalPermissionButton("Party", offPosition, onPosition, ref _global.PartyValue);
            DrawGlobalPermissionButton("PvP Team", offPosition, onPosition, ref _global.PvPTeamValue);
            DrawGlobalPermissionButton("Roleplay", offPosition, onPosition, ref _global.RoleplayValue);
            DrawGlobalPermissionButton("Say", offPosition, onPosition, ref _global.SayValue);
            DrawGlobalPermissionButton("Shout", offPosition, onPosition, ref _global.ShoutValue);
            DrawGlobalPermissionButton("Tell", offPosition, onPosition, ref _global.TellValue);
            DrawGlobalPermissionButton("Yell", offPosition, onPosition, ref _global.YellValue);
            
            ImGui.Spacing();
            
            ImGui.TextUnformatted("Linkshell Permissions");
            ImGui.Separator();
            for (uint index = 0; index < 8; index++)
                DrawGlobalLinkshellButton(index, true, offPosition, onPosition, ref _global.LinkshellValues[index]);
            
            ImGui.Spacing();
            
            ImGui.TextUnformatted("Cross-world Linkshell Permissions");
            ImGui.Separator();
            for (uint index = 0; index < 8; index++)
                DrawGlobalLinkshellButton(index, false, offPosition, onPosition, ref _global.CrossWorldLinkshellValues[index]);
        });

        SharedUserInterfaces.ContentBox("PermissionsGlobalElevated", AetherRemoteColors.PrimaryColor, true, () =>
        {
            ImGui.AlignTextToFramePadding();
            
            ImGui.TextUnformatted("Elevated Permissions"); ImGui.SameLine(offPosition);
            ImGui.TextUnformatted("Off"); ImGui.SameLine(onPosition);
            ImGui.TextUnformatted("On");
            ImGui.Separator();
            
            // DrawGlobalPermissionButton("Permanent Transformations", offPosition, onPosition, ref _global.PermanentTransformationValue);
            DrawGlobalPermissionButton("Possession", offPosition, onPosition, ref _global.PossessionValue);
        });
        
        SharedUserInterfaces.ContentBox("PermissionsGlobalSave", AetherRemoteColors.PanelColor, false, () =>
        {
            if (ImGui.Button("Save Changes", new Vector2(width - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight)))
                _ = SaveGlobalPermissions().ConfigureAwait(false);
            
            if (ImGui.IsItemHovered())
                ImGui.SetTooltip("Save your global permissions");
        });
    }
    
    /// <summary>
    ///     Draws the radio buttons to make up the three options for an individual permission
    /// </summary>
    private static void DrawGlobalPermissionButton(string label, float offPosition, float onPosition, ref bool value)
    {
        ImGui.TextUnformatted(label); 
        ImGui.SameLine(offPosition);
        
        ImGui.PushID(label);
        
        // Off button
        if (ImGui.RadioButton($"##Off", value is false))
            value = false;

        ImGui.SameLine(onPosition);
        
        // On button
        var selected = value;
        if (selected)
            ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.HealerGreen);
        
        if (ImGui.RadioButton($"##On", value))
            value = true;
        
        if (selected)
            ImGui.PopStyleColor();
        
        ImGui.PopID();
    }
    
    private static void DrawGlobalLinkshellButton(uint index, bool linkshell, float offPosition, float onPosition, ref bool value)
    {
        ImGui.TextUnformatted(linkshell ? GetLinkshellName(index) : GetCrossWorldLinkshellName(index));
        ImGui.SameLine(offPosition);
        
        ImGui.PushID(linkshell ? FriendsView.LinkshellLabels[index] : FriendsView.CrossWorldLabels[index]);
        
        if (ImGui.RadioButton($"##Off", value is false))
            value = false;
        
        ImGui.SameLine(onPosition);
        
        var selected = value;
        if (selected)
            ImGui.PushStyleColor(ImGuiCol.CheckMark, ImGuiColors.HealerGreen);
        
        if (ImGui.RadioButton($"##On", value))
            value = true;
        
        if (selected)
            ImGui.PopStyleColor();
        
        ImGui.PopID();
    }
}