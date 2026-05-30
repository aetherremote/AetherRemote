using System;
using System.Collections;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Reflection;

public partial class CustomizePlusWrapper
{
    /// <summary>
    ///     Wrapper for Customize Plus' Profiles
    /// </summary>
    private class CustomizePlusProfileWrapper
    {
        // Delegates
        private readonly Func<object, string> _profileName;
        private readonly Func<object, bool> _profileEnabled;
        private readonly Func<object, int> _profilePriority;
        private readonly Func<object, IEnumerable> _profileCharacters;
        
        /// <summary>
        ///     <inheritdoc cref="CustomizePlusProfileWrapper"/>
        /// </summary>
        private CustomizePlusProfileWrapper(
            Func<object, string> profileName,  
            Func<object, bool> profileEnabled, 
            Func<object, int> profilePriority, 
            Func<object, IEnumerable> profileCharacters)
        {
            _profileName = profileName;
            _profileEnabled = profileEnabled;
            _profilePriority = profilePriority;
            _profileCharacters = profileCharacters;
        }

        /// <summary>
        ///     Create a new wrapper for customize plus profiles.
        /// </summary>
        /// <remarks>This is only intended to be called by <see cref="Reflection.CustomizePlusWrapper.Wrap"/></remarks>
        public static CustomizePlusProfileWrapper? Wrap(Type profileType)
        {
            // Fields, Properties, Methods
            if (profileType.GetProperty("Name", PublicInstance)is not { } profileNameMethod) return null;
            if (profileType.GetProperty("Enabled", PublicInstance) is not { } profileEnabledMethod) return null;
            if (profileType.GetProperty("Priority", PublicInstance) is not { } profilePriorityMethod) return null;
            if (profileType.GetProperty("Characters", PublicInstance) is not { } profileCharactersMethod) return null;

            // Delegates
            if (ReflectionHelper.CreateProperty<string>(profileNameMethod) is not { } profileName) return null;
            if (ReflectionHelper.CreateProperty<bool>(profileEnabledMethod) is not { } profileEnabled) return null;
            if (ReflectionHelper.CreateProperty<int>(profilePriorityMethod) is not { } profilePriority) return null;
            if (ReflectionHelper.CreateProperty<IEnumerable>(profileCharactersMethod) is not { } profileCharacters) return null;

            // Package all the delegates up in a nice little bow
            return new CustomizePlusProfileWrapper(profileName, profileEnabled, profilePriority, profileCharacters);
        }
        
        /// <summary>
        ///     Returns the name of the provided profile
        /// </summary>
        public string GetName(object profile) => _profileName(profile);
        
        /// <summary>
        ///     Returns if the profile is enabled or not
        /// </summary>
        public bool GetEnabled(object profile) => _profileEnabled(profile);
        
        /// <summary>
        ///     Returns the profile's priority
        /// </summary>
        public int GetPriority(object profile) => _profilePriority(profile);
        
        /// <summary>
        ///     Returns the list of penumbra ActorIdentifiers associated with this profile
        /// </summary>
        public IEnumerable GetCharacters(object profile) => _profileCharacters(profile);
    }
}
