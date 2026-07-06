using System.Numerics;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.Possession;

public partial class PossessionView
{
    public void Draw()
    {
        ImGui.BeginChild("PossessionContent", AetherRemoteDimensions.ContentSize, false, AetherRemoteImGui.ContentFlags);
        
        if (_configurationService.AgreedToPossession)
            DrawContent();
        else
            DrawWarning();
        
        ImGui.EndChild();
        ImGui.SameLine();
        _friendsListComponentUi.Draw();
    }

    private void DrawContent()
    {
        SharedUserInterfaces.ContentBox("PossessionFeedback", AetherRemoteColors.PanelColor, true, () =>
        {   
            SharedUserInterfaces.MediumText("(BETA) Possession");
            ImGui.TextUnformatted("Expect many bugs, lag, and strangeness...");
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
            if (_selectionManager.Selected.Count is not 1)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select only one friend", size);
                ImGui.EndDisabled();
            }
            else if (MissingPermissionsForATarget())
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", size);
                ImGui.EndDisabled();
            }
            else
            {
                if (_possessionManager.Possessing)
                {
                    if (_commandLockoutService.IsLocked)
                    {
                        ImGui.BeginDisabled();
                        ImGui.Button("Unpossess", size);
                        ImGui.EndDisabled();
                    }
                    else
                    {
                        if (ImGui.Button("Unpossess", size))
                            _ = Unpossess().ConfigureAwait(false);
                    }
                }
                else if (_possessionManager.Possessed)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("You are being possessed", size);
                    ImGui.EndDisabled();
                }
                else
                {
                    if (_commandLockoutService.IsLocked)
                    {
                        ImGui.BeginDisabled();
                        ImGui.Button("Possess", size);
                        ImGui.EndDisabled();
                    }
                    else
                    {
                        if (ImGui.Button("Possess", size))
                            _ = Possess().ConfigureAwait(false);
                    }
                }
            }
        });
    }

    private void DrawWarning()
    {
        SharedUserInterfaces.ContentBox("PossessionWarning", AetherRemoteColors.PrimaryColor, true, () =>
        {
            SharedUserInterfaces.BigTextCentered("Warning");
        });
        
        SharedUserInterfaces.ContentBox("PossessionWarningText", AetherRemoteColors.PanelColor, true, () =>
        {
            ImGui.PushTextWrapPos(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X);
            ImGui.TextWrapped("Possession is fundamentally a risky feature. While I have taken means to ensure all inputs and parameters match in-game limits, at the end of the day this feature will move your player without your input. The risk is definitely lower than other plugins that may move your character due to the inputs being genuinely real inputs from another player, so it is unlikely to cause concern, however...");
            ImGui.Spacing();
            ImGui.TextWrapped("You must acknowledge the risks of possible action being taken (suspended, banned for 'botting') against your Final Fantasy XIV account before using this feature.");
            ImGui.PopTextWrapPos();
        });
        
        SharedUserInterfaces.ContentBox("PossessionWarningAccept", AetherRemoteColors.PanelColor, false, () =>
        {
            if (ImGui.Button("I understand the risks", new Vector2(ImGui.GetWindowWidth() - AetherRemoteImGui.WindowPadding.X * 2, AetherRemoteDimensions.SendCommandButtonHeight)))
                _ = AcceptPossessionTermsOfService().ConfigureAwait(false);
        });
    }
}