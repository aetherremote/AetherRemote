using System.Numerics;
using System.Runtime.CompilerServices;
using AetherRemoteClient.UI.Style;
using AetherRemoteClient.Utils;
using Dalamud.Bindings.ImGui;
using Dalamud.Interface;
using Dalamud.Interface.Colors;

namespace AetherRemoteClient.UI.Views.Settings;

public partial class SettingsView
{
    // Const
    private const int SecretLength = 48;
    private static readonly string Check = FontAwesomeIcon.Check.ToIconString();
    private static readonly string Times = FontAwesomeIcon.Times.ToIconString();
    private static readonly Vector2 ModalSize = new(ImGui.GetIO().DisplaySize.X * 0.2f, 0);
    
    // Modals
    private string _addSecretModalSecretName = string.Empty;
    private string _addSecretModalSecretValue = string.Empty;
    
    private string _renameSecretModalSelectedSecretName = string.Empty;
    private string _renameSecretModalSecretName = string.Empty;
    private long _renameSecretModalSecretId = -1;
    
    private string _deleteSecretModalSecretName = string.Empty;
    private long _deleteSecretModalSecretId = -1;
    
    public void Draw()
    {
        if (ImGui.BeginChild("SettingsContent", Vector2.Zero, true))
        {
            if (ImGui.BeginTabBar("SettingsTabs"))
            {
                if (ImGui.BeginTabItem("Settings"))
                {
                    DrawSettings();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Secrets"))
                {
                    DrawSecrets();
                    ImGui.EndTabItem();
                }
                
                if (ImGui.BeginTabItem("Dependencies"))
                {
                    DrawDependencies();
                    ImGui.EndTabItem();
                }
                
                ImGui.EndTabBar();
            }
        }
        
        ImGui.EndChild();
    }
    
    private void DrawSettings()
    {
        SharedUserInterfaces.MediumText("Global Settings");
        
        var safeMode = _configurationService.SafeMode;
        if (ImGui.Checkbox("Safe Mode##SettingsSafeMode", ref safeMode))
            _ = SetSafeMode(safeMode).ConfigureAwait(false);
        
        var showOnDtrBar = _configurationService.ShowOnDtrBar;
        if (ImGui.Checkbox("Show on Dtr Bar##SettingsShowDtrBar", ref showOnDtrBar))
            _ = SetShowDtrBar(showOnDtrBar).ConfigureAwait(false);
        
        ImGui.Spacing();
        ImGui.Separator();
        SharedUserInterfaces.MediumText("Individual Settings");

        var secretId = _activeSessionService.SecretId;
        if (secretId is null)
        {
            ImGui.TextWrapped("To edit these settings, you must be logged in with a secret.");
            ImGui.BeginDisabled();
        }

        var autoLogin = _activeSessionService.AutoLogin;
        if (ImGui.Checkbox("Auto Login##SettingsAutoLogin", ref autoLogin))
            _ = SetAutoLogin(autoLogin).ConfigureAwait(false);
        
        if (secretId is null)
            ImGui.EndDisabled();
    }
    
    private void DrawSecrets()
    {
        ImGui.Spacing();
        
        if (_configurationService.Secrets.Count is 0)
        {
            ImGui.TextUnformatted("You have not added any secrets.");
        }
        else
        {
            // Still not a great name, but this function is in charge of making sure the 'used by' field for secrets is properly updated without killing the database
            _ = ShouldRefreshCharacterSecretUsage().ConfigureAwait(false);
            
            foreach (var secret in _configurationService.Secrets)
            {
                SharedUserInterfaces.ContentBox2($"{secret.Value.Value}", AetherRemoteColors.BackgroundColor, true, () =>
                {
                    SharedUserInterfaces.MediumText(secret.Value.Name);
                    
                    var names =_secretNamesInUse.TryGetValue(secret.Value.Id, out var namesUsingThisSecret) ? namesUsingThisSecret : [];
                    var count = names.Count;
                    
                    ImGui.Text(string.Concat("Used by ", count, count is 1 ? " character" : " characters"));
                    if (count > 0 & ImGui.IsItemHovered())
                    {
                        ImGui.BeginTooltip();
                        foreach (var name in names)
                            ImGui.Text(name);
                        ImGui.EndTooltip();
                    }
                    
                    var createdAt = $"Created at {secret.Value.CreatedAt.ToLocalTime()}";
                    var width = ImGui.CalcTextSize(createdAt);
                    ImGui.SameLine(ImGui.GetContentRegionAvail().X - width.X - AetherRemoteImGui.WindowPadding.X);
                    ImGui.Text(createdAt);
                });
            }
        }
        
        if (ImGui.Button("Add New Secret"))
            ImGui.OpenPopup("AddSecretPopup");

        ImGui.SameLine();

        var secrets = _configurationService.Secrets.Count;
        if (secrets is 0) ImGui.BeginDisabled();
        if (ImGui.Button("Rename Secret"))
            ImGui.OpenPopup("RenameSecretPopup");
        if (secrets is 0) ImGui.EndDisabled();
        
        ImGui.SameLine();
        
        if (secrets is 0) ImGui.BeginDisabled();
        if (ImGui.Button("Delete Secret"))
            ImGui.OpenPopup("DeleteSecretPopup");
        if (secrets is 0) ImGui.EndDisabled();

        if (ImGui.IsItemHovered())
            ImGui.SetTooltip("Opens a dialog to select the secret you'd like to delete");
        
        if (DrawAddSecretModal("AddSecretPopup", out var secretName, out var secretValue))
            _ = AddSecret(secretName, secretValue).ConfigureAwait(false);

        if (DrawRenameSecretModal("RenameSecretPopup", out var secretIdToRename, out var secretNameToRename))
            _ = RenameSecret(secretIdToRename, secretNameToRename).ConfigureAwait(false);
        
        if (DrawDeleteSecretModal("DeleteSecretPopup", out var secretIdToDelete))
            _ = RemoveSecret(secretIdToDelete).ConfigureAwait(false);
    }
    
    // TODO: Make "Enter Returns True" for all the modals
    
    private bool DrawAddSecretModal(string id, out string secretName, out string secretValue)
    {
        secretName = string.Empty;
        secretValue = string.Empty;
        
        var saveButtonClicked = false;
        var shouldCloseCurrentPopup = false;

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f));
        ImGui.SetNextWindowSize(ModalSize, ImGuiCond.Appearing);

        if (ImGui.BeginPopupModal(id, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar))
        {
            var width = ModalSize.X - AetherRemoteImGui.WindowPadding.X * 2;
            
            ImGui.Text("Secret Name");
            ImGui.SetNextItemWidth(width);
            ImGui.InputTextWithHint("##AddSecretModalSecretName", "Enter a name for the secret", ref _addSecretModalSecretName, 128);
            
            ImGui.Text("Secret");
            ImGui.SetNextItemWidth(width);
            ImGui.InputTextWithHint("##AddSecretModalSecretValue", "Enter the secret you got from the discord bot", ref _addSecretModalSecretValue, 128);

            ImGui.Spacing();

            var size = new Vector2((width - AetherRemoteImGui.WindowPadding.X) * 0.5f, 0);

            var length = _addSecretModalSecretValue.Length;
            
            // Secrets are 48 characters in length, so anything else should be disabled
            if (length < SecretLength) ImGui.BeginDisabled();
            if (ImGui.Button("Add Secret", size))
            {
                secretName = _addSecretModalSecretName.Trim();
                secretValue = _addSecretModalSecretValue.Trim();
                
                _addSecretModalSecretName = string.Empty;
                _addSecretModalSecretValue = string.Empty;
                
                saveButtonClicked = true;
                shouldCloseCurrentPopup = true;
            }
            if (length < SecretLength) ImGui.EndDisabled();
            
            ImGui.SameLine();
            
            if (ImGui.Button("Cancel", size))
            {
                _addSecretModalSecretName = string.Empty;
                _addSecretModalSecretValue = string.Empty;
                
                shouldCloseCurrentPopup = true;
            }
            
            if (shouldCloseCurrentPopup)
                ImGui.CloseCurrentPopup();
            
            ImGui.EndPopup();
        }

        return saveButtonClicked;
    }
    
    private bool DrawRenameSecretModal(string id, out long secretId, out string secretName)
    {
        secretId = -1;
        secretName = string.Empty;
        
        var renameButtonClicked = false;
        var shouldCloseCurrentPopup = false;

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f));
        ImGui.SetNextWindowSize(ModalSize, ImGuiCond.Appearing);

        if (ImGui.BeginPopupModal(id, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar))
        {
            var width = ModalSize.X - AetherRemoteImGui.WindowPadding.X * 2;
            
            ImGui.Text("Secret to Rename");
            ImGui.SetNextItemWidth(width);
            if (ImGui.BeginCombo("##SecretToRenameCombo", _renameSecretModalSelectedSecretName))
            {
                foreach (var secret in _configurationService.Secrets)
                    if (ImGui.Selectable(secret.Value.Name))
                    {
                        _renameSecretModalSecretId = secret.Key;
                        _renameSecretModalSelectedSecretName = secret.Value.Name;
                    }
                
                ImGui.EndCombo();
            }
            
            ImGui.Spacing();
            
            ImGui.Text("Rename To");
            ImGui.SetNextItemWidth(width);
            ImGui.InputTextWithHint("##RenameSecretModalSecretName", "Enter a new name for the secret", ref _renameSecretModalSecretName, 128);
            
            ImGui.Spacing();
            
            var size = new Vector2((width - AetherRemoteImGui.WindowPadding.X) * 0.5f, 0);
            
            var secretIdToRename = _renameSecretModalSecretId;
            if (secretIdToRename < 0) ImGui.BeginDisabled();
            if (ImGui.Button("Rename", size))
            {
                secretId = _renameSecretModalSecretId;
                secretName = _renameSecretModalSecretName;
                
                _renameSecretModalSecretId = -1;
                _renameSecretModalSecretName = string.Empty;
                _renameSecretModalSelectedSecretName = string.Empty;
                
                renameButtonClicked = true;
                shouldCloseCurrentPopup = true;
            }
            if (secretIdToRename < 0) ImGui.EndDisabled();
            
            ImGui.SameLine();
            
            if (ImGui.Button("Cancel", size))
            {
                _renameSecretModalSecretId = -1;
                _renameSecretModalSecretName = string.Empty;
                _renameSecretModalSelectedSecretName = string.Empty;
                
                shouldCloseCurrentPopup = true;
            }
            
            if (shouldCloseCurrentPopup)
                ImGui.CloseCurrentPopup();
            
            ImGui.EndPopup();
        }

        return renameButtonClicked;
    }
    
    private bool DrawDeleteSecretModal(string id, out long secretId)
    {
        secretId = -1;
        
        var deleteButtonClicked = false;
        var shouldCloseCurrentPopup = false;

        var viewport = ImGui.GetMainViewport();
        ImGui.SetNextWindowPos(viewport.GetCenter(), ImGuiCond.Appearing, new Vector2(0.5f));
        ImGui.SetNextWindowSize(ModalSize, ImGuiCond.Appearing);

        if (ImGui.BeginPopupModal(id, ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoTitleBar))
        {
            var width = ModalSize.X - AetherRemoteImGui.WindowPadding.X * 2;
            
            ImGui.Text("Secret to Delete");
            ImGui.SetNextItemWidth(width);
            if (ImGui.BeginCombo("##SecretToDeleteCombo", _deleteSecretModalSecretName))
            {
                foreach (var secret in _configurationService.Secrets)
                    if (ImGui.Selectable(secret.Value.Name))
                    {
                        _deleteSecretModalSecretId = secret.Key;
                        _deleteSecretModalSecretName = secret.Value.Name;
                    }
                
                ImGui.EndCombo();
            }
            
            ImGui.Spacing();
            
            var size = new Vector2((width - AetherRemoteImGui.WindowPadding.X) * 0.5f, 0);
            
            var secretIdToDelete = _deleteSecretModalSecretId;
            if (secretIdToDelete < 0) ImGui.BeginDisabled();
            if (ImGui.Button("Delete", size))
            {
                secretId = _deleteSecretModalSecretId;
                
                _deleteSecretModalSecretId = -1;
                _deleteSecretModalSecretName = string.Empty;
                
                deleteButtonClicked = true;
                shouldCloseCurrentPopup = true;
            }
            if (secretIdToDelete < 0) ImGui.EndDisabled();
            
            ImGui.SameLine();
            
            if (ImGui.Button("Cancel", size))
            {
                _deleteSecretModalSecretId = -1;
                _deleteSecretModalSecretName = string.Empty;
                
                shouldCloseCurrentPopup = true;
            }
            
            if (shouldCloseCurrentPopup)
                ImGui.CloseCurrentPopup();
            
            ImGui.EndPopup();
        }

        return deleteButtonClicked;
    }

    private void DrawDependencies()
    {
        ImGui.TextUnformatted("Install these plugins for the best experience with Aether Remote.");
        
        ImGui.PushFont(UiBuilder.IconFont);
        ImGui.BeginGroup();
        DrawCheckmarkOrCrossOut(_penumbraService.ApiAvailable);
        DrawCheckmarkOrCrossOut(_glamourerService.ApiAvailable);
        DrawCheckmarkOrCrossOut(_moodlesService.ApiAvailable);
        DrawCheckmarkOrCrossOut(_customizePlusService.ApiAvailable);
        DrawCheckmarkOrCrossOut(_honorificService.ApiAvailable);
        ImGui.EndGroup();
        ImGui.PopFont();
        
        ImGui.SameLine();
        
        ImGui.BeginGroup();
        ImGui.TextUnformatted("Penumbra");
        ImGui.TextUnformatted("Glamourer");
        ImGui.TextUnformatted("Moodles");
        ImGui.TextUnformatted("Customize+");
        ImGui.TextUnformatted("Honorific");
        ImGui.EndGroup();
    }
    
    /// <summary>
    ///     Assumes you have already pushed <see cref="UiBuilder.IconFont"/>
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void DrawCheckmarkOrCrossOut(bool apiAvailable)
    {
        if (apiAvailable)
            ImGui.TextColored(ImGuiColors.HealerGreen, Check);
        else
            ImGui.TextColored(ImGuiColors.DalamudRed, Times);
    }
}