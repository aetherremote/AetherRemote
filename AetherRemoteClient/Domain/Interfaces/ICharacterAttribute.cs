using System.Threading.Tasks;
using AetherRemoteClient.Managers;

namespace AetherRemoteClient.Domain.Interfaces;

/// <summary>
///     Interface for storing and applying attributes about a charter to the local player.
///     Intended for use inside the <see cref="ModManager"/> class
/// </summary>
public interface ICharacterAttribute
{
    /// <summary>
    ///     Store this attribute for future use
    /// </summary>
    public Task<bool> Store();
    
    
    /// <summary>
    ///     Apply this attribute to the local player
    /// </summary>
    public Task<bool> Apply(PermanentTransformationData data);
}