using AetherRemoteClient.Domain;
using AetherRemoteClient.Services;

namespace AetherRemoteClient.UI.Views.Overrides;

/// <summary>
///     Handles events from the <see cref="OverridesViewUi"/>
/// </summary>
public class OverridesViewUiController(OverrideService overrideService)
{
    /// <summary>
    ///     User permissions mapped to booleans for use in the UI
    /// </summary>
    public readonly BooleanUserPermissions Overrides = BooleanUserPermissions.From(overrideService.Overrides);
    private BooleanUserPermissions _original = BooleanUserPermissions.From(overrideService.Overrides);

    /// <summary>
    ///     Check if there are any unsaved override changes
    /// </summary>
    public bool PendingChanges() => _original.Equals(Overrides) is false;
    
    /// <summary>
    ///     Save changes made to the temporary overrides
    /// </summary>
    public void Save()
    {
        var converted = BooleanUserPermissions.To(Overrides);
        overrideService.Overrides.Primary = converted.Primary;
        overrideService.Overrides.Linkshell = converted.Linkshell;
        overrideService.Save();
        
        _original = BooleanUserPermissions.From(converted);
    }
}