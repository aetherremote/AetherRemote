using System.Reflection;

namespace AetherRemoteClient.Dependencies.CustomizePlus.Reflection.Domain.Containers;

public record ProfileContainer(PropertyInfo Name, PropertyInfo Enabled, PropertyInfo Priority, PropertyInfo Characters);