using System.Linq;
using System.Numerics;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Domain.Interfaces;
using AetherRemoteClient.Services;
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
        ImGui.BeginChild("OverridesContent", Vector2.Zero, false, AetherRemoteStyle.ContentFlags);
        ImGui.PushStyleVar(ImGuiStyleVar.ChildRounding, AetherRemoteStyle.Rounding);

        var width = new Vector2(ImGui.GetWindowWidth() - AetherRemoteStyle.NavBarDimensions.X, 0);
        if (ImGui.BeginChild("PauseFeatureHeader", width, false, AetherRemoteStyle.ContentFlags))
        {
            SharedUserInterfaces.ContentBox("PauseHeader", AetherRemoteStyle.PanelBackground, true, () =>
            {
                ImGui.TextUnformatted("Pausing a feature disables all incoming requests of that feature");
            });
            
            SharedUserInterfaces.ContentBox("PauseSpeakPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                ImGui.TextUnformatted("Speak Permissions");
                if (ImGui.BeginTable("GeneralSpeakPermissions", 4) is false)
                    return;
                
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.None);
                
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Say);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Yell);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Shout);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Tell);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Party);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Alliance);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.FreeCompany);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.PvPTeam);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Echo);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Roleplay);
                    
                ImGui.EndTable();
            });
            
            SharedUserInterfaces.ContentBox("PauseLinkshellPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                ImGui.TextUnformatted("Linkshell Permissions");
                if (ImGui.BeginTable("LinkshellSpeakPermissions", 4) is false)
                    return;
                
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Ls1);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Ls2);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Ls3);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Ls4);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Ls5);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Ls6);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Ls7);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Ls8);
                
                ImGui.EndTable();
            });
            
            SharedUserInterfaces.ContentBox("PauseCrossWorldPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                ImGui.TextUnformatted("Cross-world Linkshell Permissions");
                if (ImGui.BeginTable("Cross-worldLinkshellPermissions", 4) is false)
                    return;
                
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Cwl1);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Cwl2);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Cwl3);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Cwl4);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Cwl5);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Cwl6);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Cwl7);
                ImGui.TableNextColumn();
                BuildPauseButtonForSpeakFeature(SpeakPermissions2.Cwl8);
                    
                ImGui.EndTable();
            });
            
            SharedUserInterfaces.ContentBox("PauseGeneralPermissions", AetherRemoteStyle.PanelBackground, true, () =>
            {
                ImGui.TextUnformatted("General Permissions");
                if (ImGui.BeginTable("GeneralPermissions", 2) is false)
                    return;
                
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions2.Emote);
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions2.Hypnosis);
                    
                ImGui.EndTable();
            });

            SharedUserInterfaces.ContentBox("PauseAttributes", AetherRemoteStyle.PanelBackground, false, () =>
            {
                ImGui.TextUnformatted("Character Attributes");
                if (ImGui.BeginTable("CharacterAttributes", 2) is false)
                    return;
                
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions2.GlamourerCustomization);
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions2.GlamourerEquipment);
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions2.Mods);
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions2.BodySwap);
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions2.Twinning);
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions2.CustomizePlus);
                ImGui.TableNextColumn();
                BuildPauseButtonForPrimaryFeature(PrimaryPermissions2.Moodles);
                    
                ImGui.EndTable();
            });
            
            SharedUserInterfaces.ContentBox("TransformationElevatedPermissions", AetherRemoteStyle.ElevatedBackground, true, () =>
            {
                ImGui.TextUnformatted("Character Attributes");
                if (ImGui.BeginTable("CharacterAttributes", 2) is false)
                    return;
                
                ImGui.TableNextColumn();
                BuildPauseButtonForElevatedFeature(ElevatedPermissions.PermanentTransformation);
                
                ImGui.EndTable();
            });
            
            ImGui.EndChild();
        }
        
        ImGui.SameLine();

        if (ImGui.BeginChild("PauseFriendHeader", Vector2.Zero, false, AetherRemoteStyle.ContentFlags))
        {
            SharedUserInterfaces.ContentBox("PauseFriendList", AetherRemoteStyle.PanelBackground, true, () =>
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
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
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
    
    private void BuildPauseButtonForSpeakFeature(SpeakPermissions2 permissions)
    {
        if (pauseService.IsFeaturePaused(permissions))
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
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

    private void BuildPauseButtonForPrimaryFeature(PrimaryPermissions2 permissions)
    {
        if (pauseService.IsFeaturePaused(permissions))
        {
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
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
            ImGui.PushStyleColor(ImGuiCol.Button, AetherRemoteStyle.PrimaryColor);
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