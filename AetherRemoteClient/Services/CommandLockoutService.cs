using System;
using System.Timers;
using AetherRemoteCommon;

namespace AetherRemoteClient.Services;

/// <summary>
///     Manages a lockout between sending commands on the UI, disabling buttons to send commands for a set time
/// </summary>
public class CommandLockoutService : IDisposable
{
    private readonly Timer _commandLockoutTimer;

    /// <summary>
    ///     <inheritdoc cref="CommandLockoutService" />
    /// </summary>
    public CommandLockoutService()
    {
        _commandLockoutTimer = new Timer();
        _commandLockoutTimer.AutoReset = false;
        _commandLockoutTimer.Elapsed += LockoutComplete;
    }

    /// <summary>
    ///     Is there an active lockout?
    /// </summary>
    public bool IsLocked { get; private set; }

    /// <summary>
    ///     Initiates a command lockout
    /// </summary>
    public void Lock(uint cooldownInSeconds = Constraints.GameCommandCooldownInSeconds)
    {
        IsLocked = true;
        _commandLockoutTimer.Stop();
        _commandLockoutTimer.Interval = cooldownInSeconds * 1000;
        _commandLockoutTimer.Start();
    }

    /// <summary>
    ///     Releases a lockout early
    /// </summary>
    public void Unlock()
    {
        IsLocked = false;
    }

    private void LockoutComplete(object? sender, ElapsedEventArgs e)
    {
        IsLocked = false;
    }
    
    public void Dispose()
    {
        _commandLockoutTimer.Elapsed -= LockoutComplete;
        _commandLockoutTimer.Dispose();
        GC.SuppressFinalize(this);
    }
}