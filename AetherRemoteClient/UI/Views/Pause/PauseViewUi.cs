using System.Linq;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
using AetherRemoteClient.Style;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain.Enums.Permissions;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

// ReSharper disable ForCanBeConvertedToForeach

namespace AetherRemoteClient.UI.Views.Pause;

public class PauseViewUi(
    PauseViewUiController controller,
    FriendsListService friendsListService, 
    PauseService pauseService) : IDrawable
{
    public void Draw()
    {
        ImGui.BeginChild("OverridesContent", Vector2.Zero, false, AetherRemoteImGui.ContentFlags);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, AetherRemoteImGui.ChildRounding);

        var width = new Vector2(ImGui.GetWindowWidth() - AetherRemoteDimensions.NavBar.X, 0);
        if (ImGui.BeginChild("PauseFeatureHeader", width, false, AetherRemoteImGui.ContentFlags))
        {
            SharedUserInterfaces.ContentBox("PauseHeader", AetherRemoteColors.PanelColor, true, () =>
            {
                ImGui.TextUnformatted("Pausing a feature disables all incoming requests of that feature");
            });
            
            SharedUserInterfaces.ContentBox("PauseSpeakPermissions", AetherRemoteColors.PanelColor, true, () =>
            {
                ImGui.TextUnformatted("Speak Permissions");
                if (ImGui.BeginTable("GeneralSpeakPermissions", 4) is false)
                    return;
                
                BuildPauseButtonForSpeakFeature(SpeakPermissions.None);
                
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Say);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Yell);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Shout);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Tell);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Party);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Alliance);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.FreeCompany);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.PvPTeam);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Echo);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Roleplay);
                    
                ImGui.EndTable();
            });
            
            SharedUserInterfaces.ContentBox("PauseLinkshellPermissions", AetherRemoteColors.PanelColor, true, () =>
            {
                ImGui.TextUnformatted("Linkshell Permissions");
                if (ImGui.BeginTable("LinkshellSpeakPermissions", 4) is false)
                    return;
                
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Ls1);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Ls2);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Ls3);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Ls4);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Ls5);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Ls6);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Ls7);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Ls8);
                
                ImGui.EndTable();
            });
            
            SharedUserInterfaces.ContentBox("PauseCrossWorldPermissions", AetherRemoteColors.PanelColor, true, () =>
            {
                ImGui.TextUnformatted("Cross-world Linkshell Permissions");
                if (ImGui.BeginTable("Cross-worldLinkshellPermissions", 4) is false)
                    return;
                
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Cwl1);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Cwl2);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Cwl3);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Cwl4);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Cwl5);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Cwl6);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Cwl7);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions.Cwl8);
                    
                ImGui.EndTable();
            });
            
            SharedUserInterfaces.ContentBox("PauseGeneralPermissions", AetherRemoteColors.PanelColor, true, () =>
            {
                ImGui.TextUnformatted("General Permissions");
                if (ImGui.BeginTable("GeneralPermissions", 2) is false)
                    return;
                
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions.Emote);
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions.Hypnosis);
                    
                ImGui.EndTable();
            });

            // TODO: Change includeEndPadding once elevated permissions are back
            SharedUserInterfaces.ContentBox("PauseAttributes", AetherRemoteColors.PanelColor, true, () =>
            {
                ImGui.TextUnformatted("Character Attributes");
                if (ImGui.BeginTable("CharacterAttributes", 2))
                {
                    ImGui.TableNextColumn();
                    BuildPauseButtonForPrimaryFeature(PrimaryPermissions.GlamourerCustomization);
                    ImGui.TableNextColumn();
                    BuildPauseButtonForPrimaryFeature(PrimaryPermissions.GlamourerEquipment);
                    ImGui.TableNextColumn();
                    BuildPauseButtonForPrimaryFeature(PrimaryPermissions.Mods);
                    ImGui.TableNextColumn();
                    BuildPauseButtonForPrimaryFeature(PrimaryPermissions.BodySwap);
                    ImGui.TableNextColumn();
                    BuildPauseButtonForPrimaryFeature(PrimaryPermissions.Twinning);
                    ImGui.TableNextColumn();
                    BuildPauseButtonForPrimaryFeature(PrimaryPermissions.Moodles);
                    ImGui.TableNextColumn();
                    BuildPauseButtonForPrimaryFeature(PrimaryPermissions.CustomizePlus);
                    ImGui.TableNextColumn();
                    BuildPauseButtonForPrimaryFeature(PrimaryPermissions.Honorific);
                    
                    ImGui.EndTable();
                }
            });
            
            SharedUserInterfaces.ContentBox("TransformationElevatedPermissions", AetherRemoteColors.PrimaryColor, false, () =>
            {
                ImGui.TextUnformatted("Character Attributes");
                if (ImGui.BeginTable("CharacterAttributes", 2) is false)
                    return;

                ImGui.TableNextColumn();
                BuildPauseButtonForElevatedFeature(ElevatedPermissions.Possession);

                ImGui.EndTable();
            });
            
            ImGui.EndChild();
        }
        
        ImGui.SameLine();

        if (ImGui.BeginChild("PauseFriendHeader", Vector2.Zero, false, AetherRemoteImGui.ContentFlags))
        {
            SharedUserInterfaces.ContentBox("PauseFriendList", AetherRemoteColors.PanelColor, true, () =>
            {
                ImGui.TextUnformatted("Pause Friend");
                
                ImGui.SetNextItemWidth(ImGui.GetWindowWidth() - ImGui.GetCursorPos().X - ImGui.GetStyle().WindowPadding.X);
                ImGui.InputTextWithHint("##SearchFriendInputText", "Search", ref controller.SearchString, 1000);
            });

            if (ImGui.BeginChild("PauseFriendBody", Vector2.Zero, true))
            {
                var sorted = friendsListService.Friends.OrderBy(f => f.NoteOrFriendCode).ToList();
                for (var i = 0; i < sorted.Count; i++)
                    BuildPauseButtonForFriend(sorted[i]);
                
                ImGui.EndChild();
            }

            ImGui.EndChild();
        }
        
        ImGui.PopStyleVar();
        ImGui.EndChild();
    }

    private void BuildPauseButtonForFriend(Friend friend)
    {
        if (pauseService.IsFriendPaused(friend.FriendCode))
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play, null, null, friend.FriendCode))
                pauseService.ToggleFriend(friend.FriendCode);
            ImGui.PopStyleColor();
        }
        else
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Pause, null, null, friend.FriendCode))
                pauseService.ToggleFriend(friend.FriendCode);
        }

        ImGui.SameLine();
        ImGui.TextUnformatted(friend.NoteOrFriendCode);
    }
    
    private void BuildPauseButtonForSpeakFeature(SpeakPermissions permissions)
    {
        if (pauseService.IsFeaturePaused(permissions))
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play, null, null, permissions.ToString()))
                pauseService.ToggleFeature(permissions);
            ImGui.PopStyleColor();
        }
        else
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Pause, null, null, permissions.ToString()))
                pauseService.ToggleFeature(permissions);
        }

        ImGui.SameLine();
        ImGui.TextUnformatted(permissions.ToString());
    }

    private void BuildPauseButtonForPrimaryFeature(PrimaryPermissions permissions)
    {
        if (pauseService.IsFeaturePaused(permissions))
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play, null, null, permissions.ToString()))
                pauseService.ToggleFeature(permissions);
            ImGui.PopStyleColor();
        }
        else
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Pause, null, null, permissions.ToString()))
                pauseService.ToggleFeature(permissions);
        }

        ImGui.SameLine();
        ImGui.TextUnformatted(permissions.ToString());
    }
    
    private void BuildPauseButtonForElevatedFeature(ElevatedPermissions permissions)
    {
        if (pauseService.IsFeaturePaused(permissions))
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteColors.PrimaryColor);
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Play, null, null, permissions.ToString()))
                pauseService.ToggleFeature(permissions);
            ImGui.PopStyleColor();
        }
        else
        {
            if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Pause, null, null, permissions.ToString()))
                pauseService.ToggleFeature(permissions);
        }

        ImGui.SameLine();
        ImGui.TextUnformatted(permissions.ToString());
    }
}