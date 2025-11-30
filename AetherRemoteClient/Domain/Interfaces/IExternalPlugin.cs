using System;
using System.Threading.Tasks;

namespace AetherRemoteClient.Domain.Interfaces;

/// <summary>
///     Interface for an external plugin, and calling the IPCs inside it
/// </summary>
public interface IExternalPlugin
{
    /// <summary>
    ///     Function that should be called periodically to test plugin availability
    /// </summary>
    public Task<bool> TestIpcAvailability();

    /// <summary>
    ///     Event fired when an exposed plugin is ready for use
    /// </summary>
    public event EventHandler IpcReady;
}