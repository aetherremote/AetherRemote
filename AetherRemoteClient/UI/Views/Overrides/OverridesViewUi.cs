using System.Numerics;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Overrides;

public class OverridesViewUi(OverrideService overrideService) : IDrawable
{
    private readonly OverridesViewUiController _controller = new(overrideService);

    public bool Draw()
    {
        ImGui.BeginChild("OverridesContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            SharedUserInterfaces.MediumText("Temporary Overrides");
            ImGui.TextUnformatted("Overrides ignore incoming commands from friends without changing permissions");
        });

        if (SharedUserInterfaces.ContextBoxButton(FontAwesomeIcon.Save, ImGui.GetStyle().WindowPadding, ImGui.GetWindowWidth()))
            _controller.Save();
        SharedUserInterfaces.Tooltip("Save");
        
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            ImGui.TextUnformatted("Speak");
            ImGui.Checkbox("Allow##Speak", ref _controller.Overrides.Speak);

            ImGui.TextUnformatted("Channels");
            if (ImGui.BeginTable("ChannelsContent", 4))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Say", ref _controller.Overrides.Say);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Yell", ref _controller.Overrides.Yell);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Shout", ref _controller.Overrides.Shout);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Tell", ref _controller.Overrides.Tell);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Party", ref _controller.Overrides.Party);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Alliance", ref _controller.Overrides.Alliance);
                ImGui.TableNextColumn();
                ImGui.Checkbox("FreeCompany", ref _controller.Overrides.FreeCompany);
                ImGui.TableNextColumn();
                ImGui.Checkbox("PvPTeam", ref _controller.Overrides.PvPTeam);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Echo", ref _controller.Overrides.Echo);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Chat Emote", ref _controller.Overrides.ChatEmote);
                ImGui.EndTable();
            }

            ImGui.TextUnformatted("Linkshells");
            if (ImGui.BeginTable("LinkshellContent", 8))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("1##Ls", ref _controller.Overrides.Ls1);
                ImGui.TableNextColumn();
                ImGui.Checkbox("2##Ls", ref _controller.Overrides.Ls2);
                ImGui.TableNextColumn();
                ImGui.Checkbox("3##Ls", ref _controller.Overrides.Ls3);
                ImGui.TableNextColumn();
                ImGui.Checkbox("4##Ls", ref _controller.Overrides.Ls4);
                ImGui.TableNextColumn();
                ImGui.Checkbox("5##Ls", ref _controller.Overrides.Ls5);
                ImGui.TableNextColumn();
                ImGui.Checkbox("6##Ls", ref _controller.Overrides.Ls6);
                ImGui.TableNextColumn();
                ImGui.Checkbox("7##Ls", ref _controller.Overrides.Ls7);
                ImGui.TableNextColumn();
                ImGui.Checkbox("8##Ls", ref _controller.Overrides.Ls8);
                ImGui.EndTable();
            }

            ImGui.TextUnformatted("Cross-world Linkshells");
            if (ImGui.BeginTable("CrossWorldLinkshellContent", 8))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("1##Cwl1", ref _controller.Overrides.Cwl1);
                ImGui.TableNextColumn();
                ImGui.Checkbox("2##Cwl2", ref _controller.Overrides.Cwl2);
                ImGui.TableNextColumn();
                ImGui.Checkbox("3##Cwl3", ref _controller.Overrides.Cwl3);
                ImGui.TableNextColumn();
                ImGui.Checkbox("4##Cwl4", ref _controller.Overrides.Cwl4);
                ImGui.TableNextColumn();
                ImGui.Checkbox("5##Cwl5", ref _controller.Overrides.Cwl5);
                ImGui.TableNextColumn();
                ImGui.Checkbox("6##Cwl6", ref _controller.Overrides.Cwl6);
                ImGui.TableNextColumn();
                ImGui.Checkbox("7##Cwl7", ref _controller.Overrides.Cwl7);
                ImGui.TableNextColumn();
                ImGui.Checkbox("8##Cwl8", ref _controller.Overrides.Cwl8);
                ImGui.EndTable();
            }
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            ImGui.TextUnformatted("Emotes");
            ImGui.Checkbox("Allow##Emotes", ref _controller.Overrides.Emote);
        });

        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            ImGui.TextUnformatted("Transformation");
            if (ImGui.BeginTable("CrossWorldLinkshellContent", 4))
            {
                ImGui.TableNextColumn();
                ImGui.Checkbox("Customization", ref _controller.Overrides.Customization);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Equipment", ref _controller.Overrides.Equipment);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Body Swap", ref _controller.Overrides.BodySwap);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Twinning", ref _controller.Overrides.Twinning);
                ImGui.TableNextColumn();
                ImGui.Checkbox("Customize+", ref _controller.Overrides.CustomizePlus);
                ImGui.EndTable();
            }

            ImGui.TextUnformatted("Mod Swapping");
            ImGui.Checkbox("Allow##Mods", ref _controller.Overrides.Mods);
        });
        
        SharedUserInterfaces.ContentBox(AetherRemoteStyle.PanelBackground, () =>
        {
            ImGui.TextUnformatted("Moodles");
            ImGui.Checkbox("Allow##Moodles", ref _controller.Overrides.Moodles);
        }, true, false);

        if (_controller.PendingChanges())
        {
            var pos = ImGui.GetWindowPos();
            ImGui.GetWindowDrawList().AddRect(pos, pos + ImGui.GetWindowSize(),
                ImGui.ColorConvertFloat4ToU32(ImGuiColors.DalamudOrange), AetherRemoteStyle.Rounding);
        }

        ImGui.EndChild();
        return false;
    }
}