namespace AetherRemoteCommon;

/// <summary>
///     Various constraints used throughout the plugin
/// </summary>
public static class Constraints
{
    /// <summary>
    ///     Limit the number of players targetable with in-game operations temporarily
    /// </summary>
    public const uint MaximumTargetsForInGameOperations = 3;

    /// <summary>
    ///     How many seconds in between each command must a user wait to use in-game commands (Speak, Emote, etc.)
    /// </summary>
    public const uint GlobalCommandCooldownInSeconds = 2;
    
    // Friend Code
    public const int FriendCodeMinimumLength = 2;
    public const int FriendCodeMaximumLength = 16;

    /// <summary>
    ///     Constraints relating to the speak command
    /// </summary>
    public static class Speak
    {
        public const int MessageMin = 1;
        public const int MessageMax = 440;

        public const int MessageExtraMin = 1;
        public const int MessageExtraMax = 40;
    }

    /// <summary>
    ///     Constraints relating to the hypnosis command
    /// </summary>
    public static class Hypnosis
    {
        public const int ArmsMin = 1;
        public const int ArmsMax = 5;
    
        public const int TurnsMin = 1;
        public const int TurnsMax = 10;
    
        public const int CurvesMin = 1;
        public const int CurvesMax = 10;
    
        public const int ThicknessMin = 1;
        public const int ThicknessMax = 10;
    
        public const int SpeedMin = 0;
        public const int SpeedMax = 10;
    
        public const int TextDelayMin = 0;
        public const int TextDelayMax = 10;
    
        public const int TextDurationMin = 0;
        public const int TextDurationMax = 10;
    
        public const int TextWordsMin = 0;
        public const int TextWordsMax = 2024;
    }

    /// <summary>
    ///     Constraints relating to possession
    /// </summary>
    public static class Possession
    {
        public const float VerticalMax = 45 * MathF.PI / 180f;
        public const float VerticalMin = -85 * MathF.PI / 180f;
        public const float VerticalDelta = VerticalMax - VerticalMin;

        public const float ZoomMax = 20f;
        public const float ZoomMin = 1.5f;
        public const float ZoomDelta = ZoomMax - ZoomMin;

        public const float HorizontalMax = MathF.PI;
        public const float HorizontalMin = -MathF.PI;
        public const float HorizontalDelta = HorizontalMax - HorizontalMin;
    }
}
