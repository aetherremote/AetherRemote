using System;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Responsible for managing all local client data such as friend code, friend list, etc.
/// </summary>
public class ClientDataManager : IDisposable
{
    /// <summary>
    /// Local Client's Friend Code
    /// </summary>
    public string? FriendCode;

    /// <summary>
    /// Is the plugin in safe mode?
    /// </summary>
    public bool SafeMode;

    /// <summary>
    /// Local Client's Friends List
    /// </summary>
    public FriendsList FriendsList;

    /// <summary>
    /// Local Client's Targets
    /// </summary>
    public TargetManager TargetManager;

    /// <summary>
    /// <inheritdoc cref="ClientDataManager"/>
    /// </summary>
    public ClientDataManager()
    {
        FriendCode = null;
        SafeMode = false;
        FriendsList = new();
        TargetManager = new(FriendsList);
    }

    public void Dispose()
    {
        TargetManager.Dispose();
        GC.SuppressFinalize(this);
    }
}
