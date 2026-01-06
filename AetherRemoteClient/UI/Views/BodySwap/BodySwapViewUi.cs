using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Managers;
using AetherRemoteClient.Services;
using AetherRemoteClient.UI.Components.Friends;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace AetherRemoteClient.UI.Views.BodySwap;

public class BodySwapViewUi(FriendsListComponentUi friendsList, BodySwapViewUiController controller, CommandLockoutService commandLockout, SelectionManager selection) : IDrawable
{
    // Const
    private const int InitiateBodySwapButtonHeight = 40;
    private const string TutorialText = "This feature requires you to be using a syncing service with your targets, as well as be within rendering distance. You can undo the effects of this request by going to the status tab, or reverting yourself to game or automation in glamourer.";

    // Tooltip Text
    private const string RequiresGlamourer = "Requires Glamourer plugin for you and your targets";
    private const string RequiresPenumbra = "Requires Penumbra plugin for you and your targets";
    private const string RequiresMoodles = "Requires Moodles plugin for you and your targets";
    private const string RequiresCustomize = "Requires Customize plugin for you and your targets";
    private const string RequiresHonorific = "Requires Customize plugin for you and your targets";
    
    public void Draw()
    {
        ImGui.BeginChild("BodySwapContent", AetherRemoteStyle.ContentSize, false, AetherRemoteStyle.ContentFlags);

        var width = ImGui.GetWindowWidth();
        var height = ImGui.GetWindowHeight();
        var padding = ImGui.GetStyle().WindowPadding.X;
        
        var size = ImGui.CalcTextSize(TutorialText, false, width - padding * 2f);
        
        var headerHeight = size.Y + ImGui.GetFontSize() + padding * 3f;
        var footerHeight = InitiateBodySwapButtonHeight + padding * 2f;
        var contentHeight = height - headerHeight - footerHeight - padding * 2f;
        
        if (ImGui.BeginChild("BodySwapTutorial", new Vector2(0, headerHeight), true))
        {
            SharedUserInterfaces.TextCentered("Tutorial");
            ImGui.Spacing();
            ImGui.TextWrapped(TutorialText);
            
            ImGui.EndChild();
        }
        
        ImGui.Spacing();
        
        if (ImGui.BeginChild("BodySwapOptions", new Vector2(0, contentHeight), true))
        {
            var rowOneButtonWidth = (width - padding * 3) * 0.5f;
            var rowTwoButtonWidth = (width - padding * 4) * 0.33333f;
            
            SharedUserInterfaces.MediumText("Always applied");
            DrawAttributeButton(FontAwesomeIcon.User, rowOneButtonWidth, "Customization", true, RequiresGlamourer);
            ImGui.SameLine();
            DrawAttributeButton(FontAwesomeIcon.Tshirt,rowOneButtonWidth, "Equipment", true, RequiresGlamourer);

            SharedUserInterfaces.MediumText("Extra attributes");
            if (DrawAttributeButton(FontAwesomeIcon.Wrench, rowTwoButtonWidth, "Mods", controller.SwapMods, RequiresPenumbra))
                controller.SwapMods = !controller.SwapMods;
            ImGui.SameLine();
            if (DrawAttributeButton(FontAwesomeIcon.Icons, rowTwoButtonWidth,"Moodles", controller.SwapMoodles, RequiresMoodles))
                controller.SwapMoodles = !controller.SwapMoodles;
            ImGui.SameLine();
            if (DrawAttributeButton(FontAwesomeIcon.Plus, rowTwoButtonWidth,"Customize+", controller.SwapCustomizePlus, RequiresCustomize))
                controller.SwapCustomizePlus = !controller.SwapCustomizePlus;
            ImGui.Spacing();
            if (DrawAttributeButton(FontAwesomeIcon.Crown, rowTwoButtonWidth,"Honorific", controller.SwapHonorific, RequiresHonorific))
                controller.SwapHonorific = !controller.SwapHonorific;
            
            ImGui.EndChild();
        }
        
        ImGui.Spacing();
        
        if (ImGui.BeginChild("BodySwapSend", new Vector2(0, footerHeight), true))
        {
            if (selection.Selected.Count is 0)
            {
                ImGui.BeginDisabled();
                ImGui.Button("You must select at least one friend", new Vector2(ImGui.GetWindowWidth() - padding * 2, InitiateBodySwapButtonHeight));
                ImGui.EndDisabled();
            }
            else if (controller.MissingPermissionsForATarget())
            {
                ImGui.BeginDisabled();
                ImGui.Button("You lack permissions for one or more of your targets", new Vector2(ImGui.GetWindowWidth() - padding * 2, InitiateBodySwapButtonHeight));
                ImGui.EndDisabled();
            }
            else
            {
                if (commandLockout.IsLocked)
                {
                    ImGui.BeginDisabled();
                    ImGui.Button("Please wait", new Vector2(ImGui.GetWindowWidth() - padding * 2, InitiateBodySwapButtonHeight));
                    ImGui.EndDisabled();
                }
                else
                {
                    if (ImGui.Button("Swap (Include Self)", new Vector2(width * 0.5f - padding * 2, InitiateBodySwapButtonHeight)))
                        controller.SwapBodiesIncludingSelf();

                    if (ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        ImGui.Text("Make sure you do not have yourself selected when using this");
                        ImGui.EndTooltip();
                    }
                    
                    ImGui.SameLine();

                    if (ImGui.Button("Swap", new Vector2(ImGui.GetWindowWidth() - ImGui.GetCursorPosX() - padding, InitiateBodySwapButtonHeight)))
                        controller.SwapBodies();
                }
            }
            
            ImGui.EndChild();
        }
        
        ImGui.EndChild();
        ImGui.SameLine();
        friendsList.Draw();
    }
    
    private static bool DrawAttributeButton(FontAwesomeIcon icon, float width, string text, bool selected, string? tooltip)
    {
        var font = ImGui.GetFontSize();
        var padding = ImGui.GetStyle().WindowPadding.X;

        var size = new Vector2(width, (font + padding) * 2f);
        var label = "\n" + text;
        
        ImGui.BeginGroup();
        
        bool pressed;
        if (selected)
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
            pressed = ImGui.Button(label, size);
            ImGui.PopStyleColor();
        }
        else
        {
            pressed = ImGui.Button(label, size);
        }

        var iconRect = ImGui.GetItemRectMin();
        iconRect.X += (width - font) * 0.5f;
        iconRect.Y += padding;
        
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.SetCursorScreenPos(iconRect);
        ImGui.TextUnformatted(icon.ToIconString());
        ImGui.PopFont();
        
        ImGui.EndGroup();
        
        if (tooltip is not null && ImGui.IsItemHovered())
            ImGui.SetTooltip(tooltip);

        return pressed;
    }
}