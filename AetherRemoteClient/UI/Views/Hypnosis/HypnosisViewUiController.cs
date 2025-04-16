using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Timers;
using AetherRemoteClient.Services;
using AetherRemoteClient.Utils;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums;
using AetherRemoteCommon.Domain.Network;
using ImGuiNET;

namespace AetherRemoteClient.UI.Views.Hypnosis;

public class HypnosisViewUiController
{
    // TODO: This class is so messy.. will clean up eventually... Fu fu fu~
    private const float SpiralSpeedMax = 0.003f;
    private const float SpiralSpeedMin = 0;
    private const string SpiralName = "spiral.png";
    private const float SpiralScalar = 0.5f * SpiralPreviewZoom;
    private const float SpiralPreviewZoom = 15f;
    private const float TwoPi = MathF.PI * 2f;
    
    public static readonly Vector2 SpiralSize = new(120);
    private static readonly Random Random = new();
    private static readonly int[] PreviewSpeeds = [ 10000, 6000, 4000, 3000, 2000, 1200, 800, 400, 200, 100 ];
    
    public Vector4 SpiralColor = new(1.0f, 0.25f, 1.0f, 0.5f);
    
    public Vector4 PreviewTextColor = Vector4.One;
    public string PreviewText = string.Empty;
    public int PreviewTextInterval = 5;
    public int PreviewTextMode = 0;

    private readonly Timer _currentDisplayPhraseTimer;
    private string _currentDisplayPhrase = string.Empty;
    private int _lastDisplayPhraseIndex;
    private string[] _hypnosisTextWordBank = [];
    
    private readonly string _spiralPath = Path.Combine(Plugin.PluginInterface.AssemblyLocation.Directory?.FullName!, SpiralName);
    private readonly Vector2[] _spiralUv;
    private readonly Vector2[] _corners;
    
    public int SpiralSpeed = 50;
    private float _spiralRotation;
    public int SpiralDuration;
    
    private readonly FriendsListService _friendsListService;
    private readonly NetworkService _networkService;
    private readonly SpiralService _spiralService;

    public HypnosisViewUiController(FriendsListService friendsListService, NetworkService networkService, SpiralService spiralService)
    {
        _friendsListService = friendsListService;
        _networkService = networkService;
        _spiralService = spiralService;
        _currentDisplayPhraseTimer = new Timer(SpiralSpeed);
        _currentDisplayPhraseTimer.Interval = PreviewSpeeds[PreviewTextInterval];
        _currentDisplayPhraseTimer.Elapsed += ChangeDisplayPhrase;
        _currentDisplayPhraseTimer.AutoReset = true;
        _currentDisplayPhraseTimer.Start();

        _corners =
        [
            new Vector2(-SpiralSize.X, -SpiralSize.Y) * SpiralScalar,
            new Vector2(SpiralSize.X, -SpiralSize.Y) * SpiralScalar,
            new Vector2(SpiralSize.X, SpiralSize.Y) * SpiralScalar,
            new Vector2(-SpiralSize.X, SpiralSize.Y) * SpiralScalar
        ];

        _spiralUv =
        [
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        ];
    }

    private void ChangeDisplayPhrase(object? sender, ElapsedEventArgs e)
    {
        switch (_hypnosisTextWordBank.Length)
        {
            case 0:
                _currentDisplayPhrase = string.Empty;
                return;
            case 1:
                _currentDisplayPhrase = _hypnosisTextWordBank[0];
                return;
        }

        if (PreviewTextMode is 0)
        {
            var next = Random.Next(0, _hypnosisTextWordBank.Length);
            while (next == _lastDisplayPhraseIndex)
                next = Random.Next(0, _hypnosisTextWordBank.Length);

            _lastDisplayPhraseIndex = next;
            _currentDisplayPhrase = _hypnosisTextWordBank[next];
        }
        else
        {
            _lastDisplayPhraseIndex++;
            _lastDisplayPhraseIndex %= _hypnosisTextWordBank.Length;
            _currentDisplayPhrase = _hypnosisTextWordBank[_lastDisplayPhraseIndex];
        }
    }

    public void RenderPreviewSpiral()
    {
        var spiral = Plugin.TextureProvider.GetFromFile(_spiralPath).GetWrapOrDefault();
        if (spiral is null)
            return;

        var speed = (SpiralSpeedMax - SpiralSpeedMin) * (SpiralSpeed * 0.01f) + SpiralSpeedMin;
        _spiralRotation += Plugin.Framework.UpdateDelta.Milliseconds * speed;
        _spiralRotation %= TwoPi;

        var drawList = ImGui.GetForegroundDrawList();
        var topLeft = ImGui.GetCursorScreenPos();
        var center = topLeft + SpiralSize * 0.5f;
        
        var cos = MathF.Cos(_spiralRotation);
        var sin = MathF.Sin(_spiralRotation);

        var rotated = new Vector2[4];
        for (var i = 0; i < 4; i++)
        {
            var x = _corners[i].X;
            var y = _corners[i].Y;

            rotated[i] = new Vector2(
                center.X + (cos * x - sin * y),
                center.Y + (sin * x + cos * y)
            );
        }
        
        var finalColor = ImGui.ColorConvertFloat4ToU32(SpiralColor);
        drawList.PushClipRect(topLeft, topLeft + SpiralSize , true);
        drawList.AddImageQuad(spiral.ImGuiHandle, rotated[0], rotated[1], rotated[2], rotated[3], _spiralUv[0], _spiralUv[1], _spiralUv[2], _spiralUv[3], finalColor);
        drawList.PopClipRect();
        
        SharedUserInterfaces.PushMediumFont();
        var size = ImGui.CalcTextSize(_currentDisplayPhrase);
        SharedUserInterfaces.PopMediumFont();
        
        var textColor = ImGui.ColorConvertFloat4ToU32(PreviewTextColor);
        drawList.AddText(SharedUserInterfaces.MediumFontPtr, SharedUserInterfaces.MediumFontSize, center - size * 0.5f, textColor, _currentDisplayPhrase);
    }

    public void PreviewSpiral()
    {
        var spiral = new SpiralInfo
        {
            Duration = SpiralDuration,
            Speed = SpiralSpeed,
            Color = SpiralColor,
            TextColor = PreviewTextColor,
            TextMode = PreviewTextMode is 0 ? SpiralTextMode.Random : SpiralTextMode.Ordered,
            TextSpeed = PreviewSpeeds[PreviewTextInterval - 1],
            WordBank = _hypnosisTextWordBank
        };
        
        _spiralService.StartSpiral(spiral, "Spiral Preview");
    }
    
    public void UpdateWordBank()
    {
        _hypnosisTextWordBank = PreviewText.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
    }

    public void UpdatePreviewTestSpeed()
    {
        _currentDisplayPhraseTimer.Interval = PreviewSpeeds[PreviewTextInterval - 1];
    }

    public async void SendSpiral()
    {
        try
        {
            var input = new HypnosisRequest
            {
                TargetFriendCodes = _friendsListService.Selected.Select(friend => friend.FriendCode).ToList(),
                Spiral = new SpiralInfo
                {
                    Duration = SpiralDuration,
                    Speed = SpiralSpeed,
                    TextSpeed = PreviewSpeeds[PreviewTextInterval - 1],
                    Color = SpiralColor,
                    TextColor = PreviewTextColor,
                    TextMode = PreviewTextMode is 0 ? SpiralTextMode.Random : SpiralTextMode.Ordered,
                    WordBank = _hypnosisTextWordBank
                }
            };
            
            var response = await _networkService.InvokeAsync<HypnosisRequest, BaseResponse>(HubMethod.Hypnosis, input);
            if (Plugin.DeveloperMode || response.Success)
            {
                Plugin.NotificationManager.AddNotification(NotificationHelper.Success(
                    "Successfully sent spiral", string.Empty));
            }
            else
            {
                Plugin.NotificationManager.AddNotification(NotificationHelper.Warning(
                    "Unable to send spiral", response.Message));
            }
        }
        catch (Exception e)
        {
            Plugin.Log.Warning($"Failed to send spiral, {e.Message}");
        }
    }
    
    /// <summary>
    ///     Calculates the friends who you lack correct permissions to send to
    /// </summary>
    /// <returns></returns>
    public List<string> GetFriendsLackingPermissions()
    {
        var thoseWhoYouLackPermissionsFor = new List<string>();
        foreach (var selected in _friendsListService.Selected)
        {
            if (selected.PermissionsGrantedByFriend.Primary.HasFlag(PrimaryPermissions.Hypnosis) is false)
                thoseWhoYouLackPermissionsFor.Add(selected.NoteOrFriendCode);
        }
        return thoseWhoYouLackPermissionsFor;
    }
}