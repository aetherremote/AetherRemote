using System.Collections.Generic;
using AetherRemoteCommon.Domain;
using AetherRemoteCommon.Domain.Enums.Permissions;

namespace AetherRemoteClient.Services;

public class PauseService
{
    private readonly HashSet<string> _pausedFriendCodes = [];
    private PrimaryPermissions2 _pausedPrimaryPermissions = PrimaryPermissions2.None;
    private SpeakPermissions2 _pausedSpeakPermissions = SpeakPermissions2.None;
    private ElevatedPermissions _pausedElevatedPermissions = ElevatedPermissions.None;

    public bool IsFriendPaused(string friendCode) => _pausedFriendCodes.Contains(friendCode);
    
    public void ToggleFriend(string friendCode)
    {
        if (_pausedFriendCodes.Add(friendCode) is false)
            _pausedFriendCodes.Remove(friendCode);
    }

    public bool IsFeaturePaused(ResolvedPermissions permissions)
        => IsFeaturePaused(permissions.Primary) || IsFeaturePaused(permissions.Speak) || IsFeaturePaused(permissions.Elevated);
    
    public bool IsFeaturePaused(PrimaryPermissions2 permissions) 
        => (_pausedPrimaryPermissions & permissions) is not 0;
    
    public bool IsFeaturePaused(SpeakPermissions2 permissions) 
        => (_pausedSpeakPermissions & permissions) is not 0;
    
    public bool IsFeaturePaused(ElevatedPermissions permissions) 
        => (_pausedElevatedPermissions & permissions) is not 0;
    
    public void ToggleFeature(PrimaryPermissions2 permissions) 
        => _pausedPrimaryPermissions ^= permissions;
    
    public void ToggleFeature(SpeakPermissions2 permissions) 
        => _pausedSpeakPermissions ^= permissions;
    
    public void ToggleFeature(ElevatedPermissions permissions) 
        => _pausedElevatedPermissions ^= permissions;
}