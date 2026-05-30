using System;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Reflection;

public partial class CustomizePlusWrapper
{
    /// <summary>
    ///     Wrapper for Customize Plus' Ipc Character Profile
    /// </summary>
    private class CustomizePlusIpcCharacterProfileWrapper
    {
        // Delegates
        private readonly Func<object, object> _fromFullProfile;
    
        /// <summary>
        ///     <inheritdoc cref="CustomizePlusIpcCharacterProfileWrapper"/>
        /// </summary>
        private CustomizePlusIpcCharacterProfileWrapper(Func<object, object> fromFullProfile)
        {
            _fromFullProfile = fromFullProfile;
        }

        /// <summary>
        ///     Create a new wrapper for customize plus ipc character profiles.
        /// </summary>
        /// <remarks>This is only intended to be called by <see cref="Reflection.CustomizePlusWrapper.Wrap"/></remarks>
        public static CustomizePlusIpcCharacterProfileWrapper? Wrap(Type ipcCharacterProfileType)
        {
            // Fields, Properties, Methods
            if (ipcCharacterProfileType.GetMethod("FromFullProfile", PublicStatic) is not { } fromFullProfileMethod) return null;

            // Delegates
            if (ReflectionHelper.CreateFunc<object, object>(null, fromFullProfileMethod) is not { } fromFullProfile) return null;
        
            // Package all the delegates up in a nice little bow
            return new CustomizePlusIpcCharacterProfileWrapper(fromFullProfile);
        }
    
        /// <summary>
        ///     Returns the conversion of a profile to an ipc character profile
        /// </summary>
        /// <returns></returns>
        public object FromFullProfile(object profile) => _fromFullProfile(profile);
    }
}
