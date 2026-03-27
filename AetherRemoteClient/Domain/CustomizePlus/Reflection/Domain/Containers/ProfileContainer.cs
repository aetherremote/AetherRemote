using System.Reflection;

namespace AetherRemoteClient.Domain.CustomizePlus.Reflection.Domain.Containers;

public record ProfileContainer(PropertyInfo Name, PropertyInfo Enabled, PropertyInfo Priority, PropertyInfo Characters);