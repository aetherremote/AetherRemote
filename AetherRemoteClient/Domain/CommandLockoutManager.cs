using AetherRemoteCommon;
using System;
using System.Timers;

namespace AetherRemoteClient.Domain;

/// <summary>
/// Manages a lockout between sending commands on the UI, disabling buttons to send commands for a set time
/// </summary>
public class CommandLockoutManager : IDisposable
{
    /// <summary>
    /// Is there an active lockout
    /// </summary>
    public bool IsLocked { get; private set; } = false;

    private readonly Timer commandLockoutTimer;

    /// <summary>
    /// <inheritdoc cref="CommandLockoutManager"/>
    /// </summary>
    public CommandLockoutManager()
    {
        commandLockoutTimer = new Timer();
        commandLockoutTimer.AutoReset = false;
        commandLockoutTimer.Elapsed += LockoutComplete;
    }

    /// <summary>
    /// Initiates a command lockout
    /// </summary>
    public void Lock(uint cooldownInSeconds = Constraints.GameCommandCooldownInSeconds)
    {
        IsLocked = true;
        commandLockoutTimer.Stop();
        commandLockoutTimer.Interval = cooldownInSeconds * 1000;
        commandLockoutTimer.Start();
    }

    /// <summary>
    /// Releases a lockout early
    /// </summary>
    public void Unlock() => IsLocked = false;

    private void LockoutComplete(object? sender, ElapsedEventArgs e) => IsLocked = false;

    public void Dispose()
    {
        commandLockoutTimer.Elapsed -= LockoutComplete;
        commandLockoutTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}
