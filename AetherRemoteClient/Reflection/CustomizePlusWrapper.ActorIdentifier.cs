using System;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Reflection;

public partial class CustomizePlusWrapper
{
    /// <summary>
    ///     Wrapper for Customize Plus' Actor Identifiers (technically Penumbra's but...)
    /// </summary>
    private class CustomizePlusActorIdentifierWrapper
    {
        private readonly Func<object, object> _actorName;
        private readonly Func<object, ushort> _actorWorld;
        
        /// <summary>
        ///     <inheritdoc cref="CustomizePlusActorIdentifierWrapper"/>
        /// </summary>
        private CustomizePlusActorIdentifierWrapper(Func<object, object> actorName, Func<object, ushort> actorWorld)
        {
            _actorName = actorName;
            _actorWorld = actorWorld;
        }
        
        /// <summary>
        ///     Create a new wrapper for customize plus actor identifier.
        /// </summary>
        /// <remarks>This is only intended to be called by <see cref="Reflection.CustomizePlusWrapper.Wrap"/></remarks>
        public static CustomizePlusActorIdentifierWrapper? Wrap(Type profileManagerType, object profileManagerInstance)
        {
            // Since ActorManager is included in Penumbra's submodule and not Customize, we need to extract it from a method that returns it
            if (profileManagerType.GetField("_actorManager", PrivateInstance)?.GetValue(profileManagerInstance) is not { } actorManagerInstance) return null;

            // Fields, Properties, Methods
            if (actorManagerInstance.GetType().GetMethod("GetCurrentPlayer") is not { } getCurrentPlayerMethod) return null;

            // Now we can actually get the type we care about
            var actorIdentifierType = getCurrentPlayerMethod.ReturnType;

            // Fields, Properties, Methods
            if (actorIdentifierType.GetField("PlayerName", PublicInstance) is not { } playerNameMethod) return null;
            if (actorIdentifierType.GetField("HomeWorld", PublicInstance) is not { } homeWorldMethod) return null;
            
            // Delegates
            if (ReflectionHelper.CreateFieldOpen<object>(playerNameMethod) is not { } playerName) return null;
            if (ReflectionHelper.CreateFieldOpen<ushort>(homeWorldMethod) is not { } homeWorld) return null; 
            
            // Package all the delegates up in a nice little bow
            return new CustomizePlusActorIdentifierWrapper(playerName, homeWorld);
        }

        /// <summary>
        ///     Returns the name of the actor identifier
        /// </summary>
        public string? GetCharacterName(object actorIdentifier) => _actorName(actorIdentifier).ToString();
        
        /// <summary>
        ///     Returns the world id of the actor identifier
        /// </summary>
        public ushort GetCharacterWorld(object actorIdentifier) => _actorWorld(actorIdentifier);
    }
}


