using AetherRemoteClient.Accessors.Glamourer;
using AetherRemoteClient.Domain;
using AetherRemoteClient.Providers;
using AetherRemoteCommon;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Interface;
using Dalamud.Interface.Colors;
using Dalamud.Plugin.Services;
using ImGuiNET;
using System;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;

namespace AetherRemoteClient.UI.Tabs.Modules;

public class GlamourerModule : IAetherRemoteModule
{
    // Constants
    private static readonly int TransformButtonWidth = 80;

    // Dependencies
    private readonly Configuration configuration;
    private readonly GlamourerAccessor glamourerAccessor;
    private readonly NetworkProvider networkProvider;
    private readonly IClientState clientState;
    private readonly IPluginLog logger;
    private readonly ITargetManager targetManager;

    // Variables - Glamourer
    private string glamourerData = "";
    private bool applyCustomization = true;
    private bool applyEquipment = true;

    private readonly ControlTargetManager controlTargetManager;
    private readonly Timer commandLockoutTimer;
    private bool lockoutActive = false;

    public GlamourerModule(Configuration configuration, GlamourerAccessor glamourerAccessor, NetworkProvider networkProvider,
        IClientState clientState, IPluginLog logger, ITargetManager targetManager, ControlTargetManager controlTargetManager, Timer commandLockoutTimer)
    {
        this.configuration = configuration;
        this.glamourerAccessor = glamourerAccessor;
        this.networkProvider = networkProvider;
        this.clientState = clientState;
        this.logger = logger;
        this.targetManager = targetManager;

        this.controlTargetManager = controlTargetManager;
        this.commandLockoutTimer = commandLockoutTimer;
        this.commandLockoutTimer.Elapsed += EndLockout;
    }

    public void Draw()
    {
        var shouldProcessBecomeCommand = false;
        var glamourerInstalled = glamourerAccessor.IsGlamourerInstalled;

        var questionIconOffset = SharedUserInterfaces.CalcIconSize(FontAwesomeIcon.QuestionCircle);
        questionIconOffset.Y = 0;
        questionIconOffset.X *= 0.5f;

        SharedUserInterfaces.MediumTextCentered("Transformation", null, questionIconOffset);
        ImGui.SameLine();
        ImGui.AlignTextToFramePadding();
        SharedUserInterfaces.Icon(FontAwesomeIcon.QuestionCircle);
        if (ImGui.IsItemHovered())
        {
            ImGui.BeginTooltip();
            SharedUserInterfaces.TextCentered("Requires Glamourer, Optionally Mare");
            ImGui.Text("Force target friends to change their appearance and/or equipment.");
            ImGui.EndTooltip();
        }

        SharedUserInterfaces.MediumText("Glamourer", glamourerInstalled ? ImGuiColors.ParsedOrange : ImGuiColors.DalamudGrey);
        if (glamourerInstalled == false)
            ImGui.BeginDisabled();

        ImGui.SetNextItemWidth(-1 * (TransformButtonWidth + ImGui.GetStyle().WindowPadding.X));
        if (ImGui.InputTextWithHint("###GlamourerDataInput", "Enter glamourer data", ref glamourerData, Constants.GlamourerDataCharLimit, ImGuiInputTextFlags.EnterReturnsTrue))
            shouldProcessBecomeCommand = true;

        ImGui.SameLine();

        var lockout = lockoutActive;
        if (lockout) ImGui.BeginDisabled();
        if (ImGui.Button("Transform", new Vector2(TransformButtonWidth, 0)))
            shouldProcessBecomeCommand = true;
        if (lockout) ImGui.EndDisabled();

        ImGui.Spacing();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.User))
            CopyMyGlamourerData();

        SharedUserInterfaces.Tooltip("Copy my glamourer data");
        ImGui.SameLine();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Crosshairs))
            CopyTargetGlamourerData();

        SharedUserInterfaces.Tooltip("Copy my taregt's glamourer data");
        ImGui.SameLine();

        if (SharedUserInterfaces.IconButton(FontAwesomeIcon.Broom))
            glamourerData = "";

        SharedUserInterfaces.Tooltip("Clear glamourer data input field");

        ImGui.SameLine();

        var a = ImGui.CalcTextSize("Appearance").X;
        var b = ImGui.CalcTextSize("Equipment").X;
        ImGui.SetCursorPosX(ImGui.GetWindowWidth() - a - b - (ImGui.GetFontSize() * 2) - (ImGui.GetStyle().WindowPadding.X * 4) - ImGui.GetStyle().FramePadding.X);

        ImGui.Checkbox("Appearance", ref applyCustomization);
        ImGui.SameLine();
        ImGui.Checkbox("Equipment", ref applyEquipment);

        if (shouldProcessBecomeCommand && lockoutActive == false)
        {
            BeginLockout();
            _ = ProcessBecomeCommand();
        }

        if (glamourerInstalled == false)
            ImGui.EndDisabled();
    }

    private async Task ProcessBecomeCommand()
    {
        if (controlTargetManager.MinimumTargetsMet == false || glamourerData.Length == 0)
            return;

        var glamourerApplyType = GlamourerAccessor.ConvertBoolsToApplyType(applyCustomization, applyEquipment);

        var secret = configuration.Secret;
        var result = await networkProvider.Become(secret, controlTargetManager.Targets, glamourerData, glamourerApplyType);
        if (result.Success)
        {
            // TODO: Logging
        }
        else
        {
            // TODO: Logging
        }

        // Reset glamourer data
        glamourerData = "";
    }

    private void CopyMyGlamourerData()
    {
        var playerName = clientState.LocalPlayer?.Name.ToString();
        if (playerName == null)
            return;

        var data = glamourerAccessor.GetCustomization(playerName);
        if (data == null)
            return;

        glamourerData = data;
    }

    private void CopyTargetGlamourerData()
    {
        var targetName = targetManager.Target?.Name.ToString();
        if (targetName == null)
            return;

        var data = glamourerAccessor.GetCustomization(targetName);
        if (data == null)
            return;

        glamourerData = data;
    }

    private void BeginLockout()
    {
        commandLockoutTimer.Stop();
        commandLockoutTimer.Start();

        lockoutActive = true;
    }

    private void EndLockout(object? sender, ElapsedEventArgs e)
    {
        lockoutActive = false;
    }

    public void Dispose()
    {
        commandLockoutTimer.Elapsed -= EndLockout;
        GC.SuppressFinalize(this);
    }
}
