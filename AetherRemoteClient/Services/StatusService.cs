using System;
using AetherRemoteClient.Domain;

namespace AetherRemoteClient.Services;

/// <summary>
///     Keep track of what parts of your character are modified
/// </summary>
public class StatusService
{
    /// <summary>
    ///     Any glamourer or penumbra changes
    /// </summary>
    public AetherRemoteStatus? Body { get; private set; }
    
    /// <summary>
    ///     Any Customize changes
    /// </summary>
    public AetherRemoteStatus? Proportions { get; private set; }
    
    /// <summary>
    ///     Any Honorific changes
    /// </summary>
    public AetherRemoteStatus? Identity { get; private set; }
    
    /// <summary>
    ///     Any hypnosis
    /// </summary>
    public AetherRemoteStatus? Mind { get; private set; }

    /// <summary>
    ///     Any possession
    /// </summary>
    public AetherRemoteStatus? Spirit { get; private set; }
    
    /// <summary>
    ///     Set the friend who modified your body (Glamourer, Penumbra)
    /// </summary>
    public void SetFriendWhoModifiedYourBody(Friend applier)
    {
        Body = new AetherRemoteStatus(applier, DateTime.Now);
    }
    
    /// <summary>
    ///     Set the friend who modified your proportions (Customize)
    /// </summary>
    public void SetFriendWhoModifiedYourProportions(Friend applier)
    {
        Proportions = new AetherRemoteStatus(applier, DateTime.Now);
    }

    /// <summary>
    ///     Set the friend who modified your identity (Honorifics)
    /// </summary>
    public void SetFriendWhoModifiedYourIdentity(Friend applier)
    {
        Identity = new AetherRemoteStatus(applier, DateTime.Now);
    }

    /// <summary>
    ///     Set the friend who hypnotized you (Hypnosis)
    /// </summary>
    public void SetFriendWhoHypnotizedYou(Friend applier)
    {
        Mind = new AetherRemoteStatus(applier, DateTime.Now);
    }

    /// <summary>
    ///     Set the friend who possessed you (Possession)
    /// </summary>
    public void SetFriendWhoPossessedYou(Friend applier)
    {
        Spirit = new AetherRemoteStatus(applier, DateTime.Now);
    }
}
