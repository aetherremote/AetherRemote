using System;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Reflection;

public partial class CustomizePlusWrapper
{
    /// <summary>
    ///     Wrapper for Customize Plus' Actor Manager (technically penumbra's but, whatever!)
    /// </summary>
    private class CustomizePlusActorManagerWrapper
    {
        // Delegates
        private readonly Func<object?> _getCurrentPlayer;
    
        /// <summary>
        ///     <inheritdoc cref="CustomizePlusActorManagerWrapper"/>
        /// </summary>
        private CustomizePlusActorManagerWrapper(Func<object?> getCurrentPlayer)
        {
            _getCurrentPlayer = getCurrentPlayer;
        }

        /// <summary>
        ///     Create a new wrapper for the customize plus actor manager.
        /// </summary>
        /// <remarks>This is only intended to be called by <see cref="Reflection.CustomizePlusWrapper.Wrap"/></remarks>
        public static CustomizePlusActorManagerWrapper? Wrap(Type profileManagerType, object profileManagerInstance)
        {
            // Instance
            if (profileManagerType.GetField("_actorManager", PrivateInstance)?.GetValue(profileManagerInstance) is not { } actorManagerInstance) return null;

            // Fields, Properties, Methods
            if (actorManagerInstance.GetType().GetMethod("GetCurrentPlayer") is not { } getCurrentPlayerMethod) return null;
        
            // Delegates
            if (ReflectionHelper.CreateFunc<object?>(actorManagerInstance, getCurrentPlayerMethod) is not { } getCurrentPlayer) return null;

            // Package all the delegates up in a nice little bow
            return new CustomizePlusActorManagerWrapper(getCurrentPlayer);
        }
    
        /// <summary>
        ///     Returns the penumbra ActorIdentifier of the current local player character
        /// </summary>
        /// <returns></returns>
        public object? GetCurrentPlayer() => _getCurrentPlayer();
    }   
}
