namespace AetherRemoteClient.Domain.Interfaces;

/// <summary>
///     Interface for an external plugin, and calling the IPCs inside it
/// </summary>
public interface IExternalPlugin
{
    /// <summary>
    ///     Function that should be called periodically to test plugin availability
    /// </summary>
    public void TestIpcAvailability();
}