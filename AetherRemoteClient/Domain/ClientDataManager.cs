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
        TargetManager = new();

#pragma warning disable CS0162
        if (Plugin.DeveloperMode)
        {
            FriendCode = "Dev Mode";
            FriendsList.CreateOrUpdateFriend("Friend1", false);
            FriendsList.CreateOrUpdateFriend("Friend2", true);
            FriendsList.CreateOrUpdateFriend("Friend3", true);
            FriendsList.CreateOrUpdateFriend("Friend4", false);
            FriendsList.CreateOrUpdateFriend("Friend5", false);
        }
#pragma warning restore CS0162
    }

    public void Dispose()
    {
        TargetManager.Dispose();
        GC.SuppressFinalize(this);
    }
}
