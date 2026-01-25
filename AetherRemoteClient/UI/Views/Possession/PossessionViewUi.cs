using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Possession;

public class PossessionViewUi(
    FriendsListComponentUi friendsList,
    PossessionViewUiController controller,
    CommandLockoutService commandLockoutService,
    SelectionManager selectionManager): IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("PossessionContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);
        
        SharedUserInterfaces.ContentBox("PossessionFeedback", AetherRemoteColors.PanelColor, true, () =>
        {   
            SharedUserInterfaces.MediumText("Possession - Feedback Requested");
            
            ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(4, 0));
            ImGui.TextUnformatted("Click");
            ImGui.SameLine();
            ImGui.PushStyleColor(ImGuiCol.Text, AetherRemoteStyle.DiscordBlue);
            var size = ImGui.CalcTextSize("here");
            if (ImGui.Selectable("here", false, ImGuiSelectableFlags.None, size))
                PossessionViewUiController.OpenFeedbackLink();
            ImGui.PopStyleColor();
            
            ImGui.SameLine();
            ImGui.TextUnformatted("to open the Google Form in browser");
            
            ImGui.PopStyleVar();
        });

        var height = new Vector2(0, ImGui.GetWindowHeight() - AetherRemoteDimensions.SendCommandButtonHeight * 2 - AetherRemoteImGui.WindowPadding.Y * 6 - AetherRemoteImGui.ItemSpacing.Y);
        if (ImGui.BeginChild("##MoodlesContextBoxDisplay", height, true, ImGuiWindowFlags.NoScrollbar))
        {
            SharedUserInterfaces.MediumText("Requirements");
            ImGui.BulletText("(Mouse & Keyboard) Non-mouse method of turning");
            ImGui.SameLine();
            SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
            if (ImGui.IsItemHovered())
            {
                ImGui.SetNextWindowSize(AetherRemoteDimensions.Tooltip);
                ImGui.BeginTooltip();
                ImGui.TextWrapped("Due to limitations, you will not be able to control your target by holding right click as you may be used to.\n\nInstead make sure Move Left / Turn Left and Move Right / Turn Right are set in your keybindings and utilized.");
                ImGui.EndTooltip();
            }
            
            ImGui.BulletText("(Optional) Within render distance for visuals");
            
            SharedUserInterfaces.MediumText("Tutorial");
            
            ImGui.PushTextWrapPos(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X);
            ImGui.TextWrapped("When you possess someone, you will lose control of your own character and your camera will snap to your target if they are within rendering distance. While possessing someone, they will lose the ability to move or turn their camera. Your movement inputs and camera movements will instead pilot their character. When you unpossess someone, you both will regain control of your characters.");
            ImGui.Spacing();
            ImGui.TextWrapped("If you are possessing someone, you can un-possess them from this menu. If you are being possessed, you can un-possess yourself from the status menu, the \"/ar unpossess\" command, or entering safe mode.");
            ImGui.PopTextWrapPos();
            
            ImGui.Spacing();
            ImGui.TextUnformatted("Do keep in mind there is significant lag using this feature.");
            
            ImGui.EndChild();
        }
        
        ImGui.Spacing();
        
        SharedUserInterfaces.ContentBox("PossessionSend", AetherRemoteColors.PanelColor, false, () =>
        {
            var size = new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight);
            if (selectionManager.Selected.Count is not 1)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select only one friend", size);
                ImGui.EndDisabled();
            }
            else if (controller.MissingPermissionsForATarget())
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", size);
                ImGui.EndDisabled();
            }
            else
            {
                if (commandLockoutService.IsLocked)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Possess", size);
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Possess", size))
                        controller.Possess();
                }
            }
        });
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
}