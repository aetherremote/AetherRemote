using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AetherRemoteClient.Utils;

namespace AetherRemoteClient.Reflection;

public partial class CustomizePlusWrapper
{
    /// <summary>
    ///     Wrapper for Customize Plus' Profile Manager
    /// </summary>
    private class CustomizePlusProfileManagerWrapper
    {
        // Delegates
        private readonly Func<IEnumerable> _profiles;
        private readonly Func<object, object, bool> _addCharacter;
        private readonly Action<object, object> _addTemplate;
        private readonly Func<string, bool, object> _create;
        private readonly Func<object, string, bool, object> _clone;
        private readonly Action<object> _delete;
        private readonly Action<object, bool, bool> _setEnabled;
        private readonly Action<object, int> _setPriority;
        
        /// <summary>
        ///     <inheritdoc cref="CustomizePlusProfileManagerWrapper"/>
        /// </summary>
        private CustomizePlusProfileManagerWrapper(
            Func<IEnumerable> profiles,
            Func<object, object, bool> addCharacter,
            Action<object, object> addTemplate,
            Func<string, bool, object> create,
            Func<object, string, bool, object> clone,
            Action<object> delete,
            Action<object, bool, bool> setEnabled,
            Action<object, int> setPriority)
        {
            _profiles = profiles;
            _addCharacter = addCharacter;
            _addTemplate = addTemplate;
            _create = create;
            _clone = clone;
            _delete = delete;
            _setEnabled = setEnabled;
            _setPriority = setPriority;
        }

        /// <summary>
        ///     Create a new wrapper for the customize plus actor manager.
        /// </summary>
        /// <remarks>This is only intended to be called by <see cref="Reflection.CustomizePlusWrapper.Wrap"/></remarks>
        public static CustomizePlusProfileManagerWrapper? Wrap(Type profileType, Type profileManagerType, object profileManagerInstance)
        {
            // Fields, Properties, Methods
            if (profileManagerType.GetField("Profiles", PublicInstance) is not { } profilesMethod) return null;
            if (profileManagerType.GetMethod("AddCharacter", PublicInstance) is not { } addCharacterMethod) return null;
            if (profileManagerType.GetMethod("AddTemplate", PublicInstance) is not { } addTemplateMethod) return null;
            if (profileManagerType.GetMethod("Create", PublicInstance) is not { } createMethod) return null;
            if (profileManagerType.GetMethod("Clone", PublicInstance) is not { } cloneMethod) return null;
            if (profileManagerType.GetMethod("Delete", PublicInstance) is not { } deleteMethod) return null;
            if (profileManagerType.GetMethod("SetEnabled", PublicInstance, null, [profileType, typeof(bool), typeof(bool)], null) is not { } setEnabledMethod) return null;
            if (profileManagerType.GetMethod("SetPriority", PublicInstance, null, [profileType, typeof(int)], null) is not { } setPriorityMethod) return null;

            // Delegates
            if (ReflectionHelper.CreateFieldClosed<IEnumerable>(profileManagerInstance, profilesMethod) is not { } profiles) return null;
            if (ReflectionHelper.CreateFunc<object, object, bool>(profileManagerInstance, addCharacterMethod) is not { } addCharacter) return null;
            if (ReflectionHelper.CreateAction<object, object>(profileManagerInstance, addTemplateMethod) is not { } addTemplate) return null;
            if (ReflectionHelper.CreateFunc<string, bool, object>(profileManagerInstance, createMethod) is not { } create) return null;
            if (ReflectionHelper.CreateFunc<object, string, bool, object>(profileManagerInstance, cloneMethod) is not { } clone) return null;
            if (ReflectionHelper.CreateAction<object>(profileManagerInstance, deleteMethod) is not { } delete) return null;
            if (ReflectionHelper.CreateAction<object, bool, bool>(profileManagerInstance, setEnabledMethod) is not { } setEnabled) return null;
            if (ReflectionHelper.CreateAction<object, int>(profileManagerInstance, setPriorityMethod) is not { } setPriority) return null;

            // Package all the delegates up in a nice little bow
            return new CustomizePlusProfileManagerWrapper(profiles, addCharacter, addTemplate, create, clone, delete, setEnabled, setPriority);
        }
        
        /// <summary>
        ///     Returns all the current profiles in Customize+
        /// </summary>
        /// <returns></returns>
        public List<object> GetProfiles() => _profiles().Cast<object>().ToList();
        
        /// <summary>
        ///     Returns the result of adding a character to a profile
        /// </summary>
        /// <returns></returns>
        public bool AddCharacterToProfile(object profile, object actorIdentifier) => _addCharacter(profile, actorIdentifier);
        
        /// <summary>
        ///     Returns the result of adding a template to a profile
        /// </summary>
        /// <returns></returns>
        public void AddTemplateToProfile(object profile, object template) => _addTemplate(profile, template);
        
        /// <summary>
        ///     Returns a newly created profile
        /// </summary>
        /// <returns></returns>
        public object CreateProfile(string name, bool handlePath = true) => _create(name, handlePath); 
        
        /// <summary>
        ///     Returns a cloned version of another profile
        /// </summary>
        /// <returns></returns>
        public object CloneProfile(object profile, string name, bool handlePath = true) => _clone(profile, name, handlePath);
        
        /// <summary>
        ///     Deletes a profile
        /// </summary>
        /// <returns></returns>
        public void DeleteProfile(object profile) => _delete(profile);
        
        /// <summary>
        ///     Sets a profile to be enabled or disabled
        /// </summary>
        /// <returns></returns>
        public void SetProfileEnabled(object profile, bool value, bool force = false) => _setEnabled(profile, value, force);
        
        /// <summary>
        ///     Sets the priority of a profile
        /// </summary>
        /// <returns></returns>
        public void SetProfilePriority(object profile, int value) => _setPriority(profile, value);
    }    
}
