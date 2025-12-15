namespace AetherRemoteCommon.Domain.Enums;

/// <summary>
///     Represents the result of a client preforming an action
/// </summary>
public enum ActionResultEc
{
    /// <summary>
    ///     Default value, never should be encountered
    /// </summary>
    Uninitialized,
    
    /// <summary>
    ///     Client is not friends with the issuer of the action
    /// </summary>
    ClientNotFriends,
    
    /// <summary>
    ///     Client is in safe mode
    /// </summary>
    ClientInSafeMode,
    
    /// <summary>
    ///     Client is pausing this feature paused
    /// </summary>
    ClientHasFeaturePaused,
    
    /// <summary>
    ///     Client has the sender paused
    /// </summary>
    ClientHasSenderPaused,
    
    /// <summary>
    ///     Client has not granted the sender permissions for this feature
    /// </summary>
    ClientHasNotGrantedSenderPermissions,
    
    /// <summary>
    ///     Client was sent back data and was unable to parse it
    /// </summary>
    ClientBadData,
    
    /// <summary>
    ///     Client does not have a dependent plugin required for this feature
    /// </summary>
    ClientPluginDependency,
    
    /// <summary>
    ///     Client is being hypnotized by someone else
    /// </summary>
    ClientBeingHypnotized,
    
    /// <summary>
    ///     The client is permanently transformed and cannot change
    /// </summary>
    ClientPermanentlyTransformed,
    
    /// <summary>
    ///     The client's local character was not found
    /// </summary>
    ClientNoLocalPlayer,
    
    /// <summary>
    ///     Target client is offline
    /// </summary>
    TargetOffline,
    
    /// <summary>
    ///     Target is not friends with the issuer
    /// </summary>
    TargetNotFriends,
    
    /// <summary>
    ///     Target has not granted the issuer adequate permissions
    /// </summary>
    TargetHasNotGrantedSenderPermissions,
    
    /// <summary>
    ///     The request to this target timed out
    /// </summary>
    TargetTimeout,
    
    /// <summary>
    ///     Action was successful
    /// </summary>
    Success,
    
    /// <summary>
    ///     A value that should have a value provided did not
    /// </summary>
    ValueNotSet,
    
    /// <summary>
    ///     Unknown issue occurred
    /// </summary>
    Unknown
}