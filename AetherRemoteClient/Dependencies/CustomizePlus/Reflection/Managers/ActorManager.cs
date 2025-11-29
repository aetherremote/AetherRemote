using System;
using System.Reflection;

namespace AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Managers;

/// <summary>
///     Domain encapsulation of a CustomizePlus ActorManager class
/// </summary>
public class ActorManager
{
    // Injected
    private readonly object _actionManager;
    private readonly MethodInfo _getCurrentPlayer;
    
    /// <summary>
    ///     <inheritdoc cref="ActorManager"/>
    /// </summary>
    private ActorManager(object actionManager, MethodInfo getCurrentPlayer)
    {
        _actionManager = actionManager;
        _getCurrentPlayer = getCurrentPlayer;
    }

    /// <summary>
    ///     Retrieves the local character as an ActorIdentifier
    /// </summary>
    /// <returns>Penumbra actor identifier object for use in Customize internals</returns>
    public object? GetCurrentPlayer()
    {
        try
        {
            return _getCurrentPlayer.Invoke(_actionManager, null);
        }
        catch (Exception e)
        {
            return null;
        }
    }
    
    /// <summary>
    ///     Creates a new instance of the ActorManager
    /// </summary>
    /// <remarks>
    ///     Ideally this is called only a single time to not incur multiple reflection calls
    /// </remarks>
    public static ActorManager? Create(object profileManagerInstance)
    {
        try
        {
            var type = profileManagerInstance.GetType();
            if (type.GetField("_actorManager", BindingFlags.Instance | BindingFlags.NonPublic) is not { } actorManagerField)
                return null;
            
            if (actorManagerField.GetValue(profileManagerInstance) is not { } actorManager)
                return null;
            
            return actorManager.GetType().GetMethod("GetCurrentPlayer") is { } getCurrentPlayer 
                ? new ActorManager(actorManager, getCurrentPlayer) 
                : null;
        }
        catch (Exception e)
        {
            Plugin.Log.Error($"[ProfileManager.Create] An error occurred while reflecting, {e}");
            return null;
        }
    }
}