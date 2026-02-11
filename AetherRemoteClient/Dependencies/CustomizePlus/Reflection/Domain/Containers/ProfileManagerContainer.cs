using System.Reflection;

namespace AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Domain.Containers;

public record ProfileManagerContainer(
    MethodInfo AddCharacter, 
    MethodInfo AddTemplate, 
    MethodInfo Create, 
    MethodInfo Duplicate, 
    MethodInfo Delete, 
    MethodInfo SetEnabled, 
    MethodInfo SetPriority);